using System;
using System.Reflection;
using System.Data.Common;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	public class DbFieldMap<T> where T : class, new()
	{
		private List<ColumnMemberInfo> _FieldList;
		private ulong _RowCount;

		private Action<T> _CustomInitAction = null;
		private Action<T, DbDataReader> _CustomReaderAction = null;
		private bool _AutoMatchAllPropertiesOrFields = false;
		private bool _AllowAutoMatch = true;

		public DbFieldMap()
		{
			_FieldList = new List<ColumnMemberInfo>();
			_RowCount = 0;
		}

		private void MapColumns(DbDataReader dataReader)
		{
			int columnCount = dataReader.FieldCount;
			Dictionary<string, int> columnOrdinals = new Dictionary<string, int>(columnCount);

			for (int i = 0; i < columnCount; i++)
				columnOrdinals.Add(dataReader.GetName(i), i);

			SortedList<int, ColumnMemberInfo> restMembers = new SortedList<int, ColumnMemberInfo>(_FieldList.Count);
			ColumnMemberInfo field;
			int columnOrdinal;

			// 1. Case-sensitive Matching
			for (int i = 0; i < _FieldList.Count; i++)
			{
				field = _FieldList[i];

				if (columnOrdinals.TryGetValue(field.ColumnName, out columnOrdinal))
				{
					field.ColumnOrdinal = columnOrdinal;
					columnOrdinals.Remove(field.ColumnName);
				}
				else
					restMembers.Add(i, field);
			}

			if (columnOrdinals.Count > 0 && restMembers.Count > 0)
			{
				// 2. Case-insensitive Matching
				columnOrdinals = RestColumnOrdinalDictionary(columnOrdinals);

				if (columnOrdinals.Count > 0)
				{
					RetryMatchColumnWithPropertiesOrFields(columnOrdinals, restMembers);

					if (_AutoMatchAllPropertiesOrFields && columnOrdinals.Count > 0 && restMembers.Count > 0)
					{
						// 3. Compact (De-underscore) column names and then Case-insensitive Matching
						columnOrdinals = RestColumnOrdinalDictionary(columnOrdinals, colName => colName.CompactFieldName());

						if (columnOrdinals.Count > 0)
							RetryMatchColumnWithPropertiesOrFields(columnOrdinals, restMembers);
					}
				}
			}

			// 4. Discard(Ignore) Unmatched PropertiesOrFields
			for (int i = restMembers.Count - 1; i >= 0; i--)
				_FieldList.RemoveAt(restMembers.Keys[i]);
		}

		private Dictionary<string, int> RestColumnOrdinalDictionary(Dictionary<string, int> columnOrdinals, Func<string, string> nameResolver = null)
		{
			Dictionary<string, int> restColumnOrdinals = new Dictionary<string, int>(columnOrdinals.Count, StringComparer.OrdinalIgnoreCase);
			string resolvedName;

			foreach (var pair in columnOrdinals)
			{
				resolvedName = (nameResolver == null) ? pair.Key : nameResolver(pair.Key);

				if (!string.IsNullOrEmpty(resolvedName) && !restColumnOrdinals.ContainsKey(resolvedName))
					restColumnOrdinals.Add(resolvedName, pair.Value);
			}

			return restColumnOrdinals;
		}

		private void RetryMatchColumnWithPropertiesOrFields(Dictionary<string, int> columnOrdinals, SortedList<int, ColumnMemberInfo> restMembers)
		{
			ColumnMemberInfo field;
			int columnOrdinal;

			for (int i = restMembers.Count - 1; i >= 0; i--)
			{
				field = restMembers.Values[i];

				if (columnOrdinals.TryGetValue(field.ColumnName, out columnOrdinal))
				{
					field.ColumnOrdinal = columnOrdinal;
					columnOrdinals.Remove(field.ColumnName);
					restMembers.RemoveAt(i);
				}
			}
		}

		public DbFieldMap<T> Add(string columnName, Expression<Func<T, IConvertible>> fieldExpr)
		{
			_FieldList.Add(new ColumnMemberInfo(columnName, fieldExpr));
			return this;
		}

		public void CustomizeInitAction(Action<T> entityInitAction)
		{
			_CustomInitAction = entityInitAction;
		}

		public void CustomizeReaderAction(Action<T, DbDataReader> customReaderAction, bool allowAutoMatch /*allowAutoMapAllPropertiesOrFields*/ = false)
		{
			_CustomReaderAction = customReaderAction;
			_AllowAutoMatch = allowAutoMatch;
		}

		internal void AddAllPropertiesOrFields()
		{
			Type type = typeof(T);

			foreach (FieldInfo f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
				if (f.IsInitOnly == false && f.FieldType.GetNonNullableType().CanMapToDbType())
					_FieldList.Add(new ColumnMemberInfo(f.Name, f));

			foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				if (p.CanWrite && p.CanRead && p.PropertyType.GetNonNullableType().CanMapToDbType())
					_FieldList.Add(new ColumnMemberInfo(p.Name, p));

			_AutoMatchAllPropertiesOrFields = true;
		}

		internal void PrepareResultMap(Action<DbFieldMap<T>> resultMap = null)
		{
			if (resultMap != null)
				resultMap(this);
			else
				if (_AllowAutoMatch)
					AddAllPropertiesOrFields();
		}

		internal T ReadNew(DbDataReader dataReader)
		{
			T entity = new T();

			if (_CustomInitAction != null)
				_CustomInitAction(entity);

			if (_RowCount == 0L)
				MapColumns(dataReader);

			foreach (ColumnMemberInfo field in _FieldList)
				field.SetValue(entity, dataReader.GetColumnValue(field.ColumnOrdinal));

			if (_CustomReaderAction != null)
				_CustomReaderAction(entity, dataReader);

			_RowCount++;
			return entity;
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
//	Created Date:		2012-05-03
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
