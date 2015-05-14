using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.SqlServer.Server;

namespace DbParallel.DataAccess
{
	/// <summary>
	/// For supporting Table-Valued Parameter (SQL Server 2008+) and Oracle Associative Array Parameter
	/// </summary>
	public static class ParameterConvert
	{
		#region Convert IEnumerable<IDictionary<string, object>> to DataTable

		/// <summary>
		/// Creates a DataTable from an IEnumerable&lt;IDictionary&lt;string, object&gt;&gt; (collection of dynamic objects)
		/// </summary>
		/// <typeparam name="T">Dynamic object type (IDictionary&lt;string, object&gt;)</typeparam>
		/// <param name="dynObjects">A collection of dynamic objects</param>
		/// <returns>A DataTable that contains the data from the input dynamic objects' properties</returns>
		public static DataTable ToDataTable<T>(this IEnumerable<T> dynObjects) where T : IDictionary<string, object>
		{
			if (dynObjects == null)
				return null;

			DataTable dataTable = PrepareDataColumns(dynObjects);

			dataTable.BeginLoadData();
			LoadDataRows(dataTable, dynObjects);
			dataTable.EndLoadData();

			return dataTable;
		}

		private static DataTable PrepareDataColumns<T>(IEnumerable<T> dynObjects) where T : IDictionary<string, object>
		{
			DataTable dataTable = new DataTable();
			DataColumnCollection tableColumns = dataTable.Columns;
			HashSet<string> pendingColumns = new HashSet<string>();

			foreach (var dynObj in dynObjects)
			{
				foreach (var p in dynObj)
				{
					if (pendingColumns.Contains(p.Key))
					{
						if (p.Value != null && !Convert.IsDBNull(p.Value))
						{
							tableColumns[p.Key].DataType = p.Value.GetType().GetNonNullableType();
							pendingColumns.Remove(p.Key);
						}
					}
					else if (!tableColumns.Contains(p.Key))
					{
						if (p.Value == null || Convert.IsDBNull(p.Value))
						{
							tableColumns.Add(p.Key);
							pendingColumns.Add(p.Key);
						}
						else
							tableColumns.Add(p.Key, p.Value.GetType().GetNonNullableType());
					}
				}

				if (pendingColumns.Count == 0)
					break;
			}

			return dataTable;
		}

		private static void LoadDataRows<T>(DataTable dataTable, IEnumerable<T> dynObjects) where T : IDictionary<string, object>
		{
			DataRow row;

			foreach (var dynObj in dynObjects)
			{
				row = dataTable.NewRow();

				foreach (var p in dynObj)
					row[p.Key] = (p.Value == null) ? DBNull.Value : p.Value;

				dataTable.Rows.Add(row);
			}
		}

		#endregion

		#region Convert IEnumerable<object> (anonymous type instances) to DataTable

		/// <summary>
		/// Creates a DataTable from an IEnumerable&lt;anonymousObjects&gt; (collection of anonymous or named type instances)
		/// </summary>
		/// <param name="anonymousObjects">A collection of anonymous or named type instances</param>
		/// <returns>A DataTable that contains the data from the input objects' properties</returns>
		public static DataTable ToDataTable(this IEnumerable<object> anonymousObjects)
		{
			if (anonymousObjects == null)
				return null;

			DataTable dataTable = new DataTable();
			PropertyDescriptorCollection properties = null;
			int countColumns = 0;
			DataRow row;
			object cellValue;

			dataTable.BeginLoadData();

			foreach (object obj in anonymousObjects)
			{
				if (properties == null)
				{
					properties = TypeDescriptor.GetProperties(obj);
					countColumns = properties.Count;

					if (countColumns == 0)
						break;

					foreach (PropertyDescriptor prop in properties)
						dataTable.Columns.Add(prop.Name, prop.PropertyType);
				}

				row = dataTable.NewRow();

				for (int i = 0; i < countColumns; i++)
				{
					cellValue = properties[i].GetValue(obj);
					row[i] = (cellValue == null) ? DBNull.Value : cellValue;
				}

				dataTable.Rows.Add(row);
			}

			dataTable.EndLoadData();

			return dataTable;
		}

		#endregion

		#region AsParameterValue overloads

		/// <summary>
		/// Check an input value is an acceptable simple parameter type, convert to DBNull if it's null.
		/// </summary>
		/// <param name="simpleValue"></param>
		/// <returns></returns>
		public static IConvertible AsParameterValue(this IConvertible simpleValue)
		{
			return simpleValue ?? DBNull.Value;
		}

		/// <summary>
		/// Check an input value is an acceptable Oracle associative array parameter type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="associativeArray"></param>
		/// <returns></returns>
		public static T[] AsParameterValue<T>(this T[] associativeArray) where T : IConvertible
		{
			return associativeArray;	// Associative Array Parameter (Oracle)
		}

		/// <summary>
		/// Check an input value is an acceptable Table-Valued Parameter (SQL Server 2008+)
		/// </summary>
		/// <param name="tableValue"></param>
		/// <returns></returns>
		public static DataTable AsParameterValue(this DataTable tableValue)
		{
			return tableValue;			// Table-Valued Parameter (SQL Server 2008+)
		}

		/// <summary>
		/// Check an input value is an acceptable Table-Valued Parameter (SQL Server 2008+)
		/// </summary>
		/// <param name="readerValue"></param>
		/// <returns></returns>
		public static DbDataReader AsParameterValue(this DbDataReader readerValue)
		{
			return readerValue;			// Table-Valued Parameter (SQL Server 2008+)
		}

		/// <summary>
		/// Check an input collection is an acceptable parameter type, convert if necessary.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerableData"></param>
		/// <returns></returns>
		public static object AsParameterValue<T>(this IEnumerable<T> enumerableData)
		{
			if (enumerableData == null)
				return null;
			else
			{
				Type t = typeof(T);

				if (typeof(IConvertible).IsAssignableFrom(t))						// Oracle Associative Array Parameter
					return enumerableData.ToArray();
				else if (typeof(SqlDataRecord).IsAssignableFrom(t))					// Table-Valued Parameter (SQL Server 2008+) - SqlDataRecord
					return enumerableData;
				else if (typeof(IDictionary<string, object>).IsAssignableFrom(t))	// Table-Valued Parameter (SQL Server 2008+) - IDictionary<string, object>
					return enumerableData.AsOfType<IDictionary<string, object>>().ToDataTable();
				else																// Table-Valued Parameter (SQL Server 2008+) - Anonymous or named type instances
				{
					DataTable tvp = enumerableData.AsOfType<object>().ToDataTable();

					if (tvp.Columns.Count > 0)
						return tvp;
					else
						return enumerableData;
				}
			}
		}

		#endregion

		/// <summary>
		/// Try cast an IEnumerable as the specified type; If not, filters the elements of an IEnumerable based on a specified type.
		/// </summary>
		/// <typeparam name="T">The type to cast/filter the elements of source to</typeparam>
		/// <param name="source">The IEnumerable that contains the elements to be cast to type T</param>
		/// <returns>An IEnumerable&lt;T&gt; that contains each element of the source sequence cast/filter to the specified type.</returns>
		public static IEnumerable<T> AsOfType<T>(this IEnumerable source)
		{
			return (source as IEnumerable<T>) ?? source.OfType<T>();
		}

		/// <summary>
		/// <para>Normalize an object[] (promiscuous element numeric types) to a most compatible primitive type</para>
		/// <para>For example, object[] {0, 10000L, 3.14, 0.618m} ==> decimal[] {0m, 10000m, 3.14m, 0.618m}</para>
		/// </summary>
		/// <param name="rawArray">A promiscuous types' numeric array</param>
		/// <returns>A normalized new array if all elements are numeric, or just the rawArray itself if contains any non-numeric element.</returns>
		public static Array NormalizeNumericArray(this object[] rawArray)
		{
			Type compatibleType = GetNumericElementType(rawArray);

			if (compatibleType != null && compatibleType != typeof(object))
			{
				Array newArray = Array.CreateInstance(compatibleType, rawArray.Length);

				for (int i = 0; i < rawArray.Length; i++)
					newArray.SetValue(Convert.ChangeType(rawArray[i], compatibleType), i);

				return newArray;
			}

			return rawArray;
		}

		private static Type GetNumericElementType(Array arrayValue)
		{
			int weight, maxWeight = 0;
			Type mostCompatibleType = null;

			foreach (object element in arrayValue)
			{
				weight = WeighNumericType(element);

				if (weight < 0)		// Non-numeric
					return null;

				if (weight > maxWeight)
				{
					mostCompatibleType = element.GetType();
					maxWeight = weight;
				}
			}

			return mostCompatibleType;
		}

		private static int WeighNumericType(object numericObject)
		{
			if (numericObject == null || Convert.IsDBNull(numericObject))
				return 0;

			//	if (numericObject is bool)
			//		return 1;

			if (numericObject is sbyte)
				return 2;

			if (numericObject is byte)
				return 3;

			//	if (numericObject is char)
			//		return 4;

			if (numericObject is short)
				return 5;

			if (numericObject is ushort)
				return 6;

			if (numericObject is int)
				return 7;

			if (numericObject is uint)
				return 8;

			if (numericObject is long)
				return 9;

			if (numericObject is ulong)
				return 10;

			if (numericObject is float)
				return 11;

			if (numericObject is double)
				return 12;

			if (numericObject is decimal)
				return 13;

			return -1;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2015 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2015-05-08
//	Original Host:		http://dbParallel.codeplex.com
//	Primary Host:		http://DataBooster.codeplex.com
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
