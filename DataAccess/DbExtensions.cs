using System;
using System.Data;
using System.Data.Linq;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	public static partial class DbExtensions
	{
		#region Type helper
		internal static bool IsNullable(this Type type)
		{
			return (type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
		}

		internal static Type GetNonNullableType(this Type type)
		{
			return type.IsNullable() ? Nullable.GetUnderlyingType(type) : type;
		}

		internal static bool CanMapToDbType(this Type type)
		{
			if (typeof(IConvertible).IsAssignableFrom(type))
				return true;
			if (type.IsValueType)
				return true;
			if (type == typeof(string))
				return true;
			return false;
		}
		#endregion

		private static T TryConvert<T>(object dbValue)
		{
			if (dbValue == null || Convert.IsDBNull(dbValue))
				return default(T);
			else
			{
				try
				{
					return (T)dbValue;
				}
				catch (InvalidCastException)
				{
					return DBConvert.ChangeType<T>(dbValue);	//	(T)Convert.ChangeType(dbValue, typeof(T).TryUnderlyingType());
				}
			}
		}

		public static object GetColumnValue(this DbDataReader dataRecord, int ordinal)
		{
			try
			{
				return dataRecord.GetValue(ordinal);
			}
			catch (OverflowException)
			{
				string dataTypeName = dataRecord.GetDataTypeName(ordinal);

				if (dataTypeName.Equals("Decimal", StringComparison.OrdinalIgnoreCase))
				{
					decimal decimalValue;
					string strValue = dataRecord.GetProviderSpecificValue(ordinal).ToString();

					if (decimal.TryParse(strValue, out decimalValue))
						return decimalValue;
				}

				throw;
			}
		}

		public static object GetColumnValue(this DbDataReader dataRecord, string columnName)
		{
			return GetColumnValue(dataRecord, dataRecord.GetOrdinal(columnName));
		}

		public static IEnumerable GetColumnValues(this DbDataReader dataRecord, int maxColumns = -1)
		{
			if (maxColumns == 0)
				maxColumns = dataRecord.VisibleFieldCount;
			else if (maxColumns < 0)
				maxColumns = dataRecord.FieldCount;

			for (int i = 0; i < maxColumns; i++)
				yield return GetColumnValue(dataRecord, i);
		}

		public static int GetColumnValues(this DbDataReader dataRecord, object[] values)
		{
			if (values == null)
				throw new ArgumentNullException("values");

			int copy = Math.Min(values.Length, dataRecord.FieldCount);

			for (int i = 0; i < copy; i++)
				values[i] = GetColumnValue(dataRecord, i);

			return copy;
		}

		public static T Field<T>(this DbDataReader reader, string columnName)
		{
			return TryConvert<T>(reader.GetColumnValue(columnName));
		}

		public static T Field<T>(this DbDataReader reader, int ordinal)
		{
			return TryConvert<T>(reader.GetColumnValue(ordinal));
		}

		public static T Parameter<T>(this DbCommand cmd, string parameterName)
		{
			return cmd.Parameters.Parameter<T>(parameterName);
		}

		public static T Parameter<T>(this DbParameterCollection parameters, string parameterName)
		{
			return parameters[parameterName].Parameter<T>();
		}

		public static T Parameter<T>(this DbParameter parameter)
		{
			return TryConvert<T>(parameter.Value);
		}


		public static DbParameter SetDbType(this DbParameter dbParameter, DbType dbType)
		{
			dbParameter.DbType = dbType;
			return dbParameter;
		}

		public static DbParameter SetDirection(this DbParameter dbParameter, ParameterDirection parameterDirection)
		{
			dbParameter.Direction = parameterDirection;
			return dbParameter;
		}

		public static DbParameter SetIsNullable(this DbParameter dbParameter, bool isNullable)
		{
			dbParameter.IsNullable = isNullable;
			return dbParameter;
		}

		public static DbParameter SetName(this DbParameter dbParameter, string parameterName)
		{
			dbParameter.ParameterName = parameterName;
			return dbParameter;
		}

		public static DbParameter SetSize(this DbParameter dbParameter, int nSize)
		{
			dbParameter.Size = nSize;
			return dbParameter;
		}

		#region Set Parameter Value overloads

		/// <summary>
		/// Set Value of Simple Parameter
		/// </summary>
		/// <param name="dbParameter"></param>
		/// <param name="simpleValue"></param>
		/// <returns></returns>
		public static DbParameter SetValue(this DbParameter dbParameter, IConvertible simpleValue)
		{
			dbParameter.Value = simpleValue.AsParameterValue();
			return dbParameter;
		}

		/// <summary>
		/// Set Value of Associative Array Parameter (Oracle)
		/// </summary>
		/// <param name="dbParameter"></param>
		/// <param name="associativeArray"></param>
		/// <returns></returns>
		public static DbParameter SetValue<T>(this DbParameter dbParameter, T[] associativeArray) where T : IConvertible
		{
			dbParameter.Value = associativeArray.AsParameterValue();
			return dbParameter;
		}

		/// <summary>
		/// Set Value of Table-Valued Parameter (SQL Server 2008+)
		/// </summary>
		/// <param name="dbParameter"></param>
		/// <param name="tableValue"></param>
		/// <returns></returns>
		public static DbParameter SetValue(this DbParameter dbParameter, DataTable tableValue)
		{
			dbParameter.Value = tableValue.AsParameterValue();
			return dbParameter;
		}

		/// <summary>
		/// Set Value of Table-Valued Parameter (SQL Server 2008+)
		/// </summary>
		/// <param name="dbParameter"></param>
		/// <param name="readerValue"></param>
		/// <returns></returns>
		public static DbParameter SetValue(this DbParameter dbParameter, DbDataReader readerValue)
		{
			dbParameter.Value = readerValue.AsParameterValue();
			return dbParameter;
		}

		/// <summary>
		/// Set Value of Table-Valued Parameter (SQL Server 2008+) or Associative Array Parameter (Oracle)
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="dbParameter"></param>
		/// <param name="enumerableData"></param>
		/// <returns></returns>
		public static DbParameter SetValue<T>(this DbParameter dbParameter, IEnumerable<T> enumerableData)
		{
			dbParameter.Value = enumerableData.AsParameterValue();
			return dbParameter;
		}

		#endregion

		public static DbParameter SetPrecision(this DbParameter dbParameter, byte nPrecision)
		{
			IDbDataParameter iDbDataParameter = dbParameter as IDbDataParameter;
			iDbDataParameter.Precision = nPrecision;
			return dbParameter;
		}

		public static DbParameter SetScale(this DbParameter dbParameter, byte nScale)
		{
			IDbDataParameter iDbDataParameter = dbParameter as IDbDataParameter;
			iDbDataParameter.Scale = nScale;
			return dbParameter;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2012 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2012-03-23
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
