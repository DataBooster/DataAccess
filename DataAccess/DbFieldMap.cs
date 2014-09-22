using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data.Common;
using System.Linq.Expressions;

namespace DbParallel.DataAccess
{
	public class DbFieldMap<T> where T : class, new()
	{
		private List<ColumnMemberInfo> _FieldList;
		private ulong _RowCount;

		private Action<T> _CustomInitAction = null;
		private Action<T, DbDataReader> _CustomReaderAction = null;
		private bool _AllowAutoMatch = true;

		public DbFieldMap()
		{
			_FieldList = new List<ColumnMemberInfo>();
			_RowCount = 0;
		}

		private void MapColumns(DbDataReader dataReader)
		{
			ColumnMemberInfo field;

			for (int i = _FieldList.Count - 1; i >= 0; i--)
			{
				field = _FieldList[i];

				try
				{
					field.ColumnOrdinal = dataReader.GetOrdinal(field.ColumnName);
				}
				catch (IndexOutOfRangeException)
				{
					_FieldList.RemoveAt(i);
				}
			}
		}

		public DbFieldMap<T> Add(string columnName, Expression<Func<T, object>> fieldExpr)
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

			foreach (PropertyInfo p in type.GetProperties())
				if (p.CanWrite && p.CanRead && p.PropertyType.TryUnderlyingType().CanMapToDbType())
					_FieldList.Add(new ColumnMemberInfo(p.Name, p));

			foreach (FieldInfo f in type.GetFields())
				if (f.IsInitOnly == false && f.FieldType.TryUnderlyingType().CanMapToDbType())
					_FieldList.Add(new ColumnMemberInfo(f.Name, f));
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
				field.SetValue(entity, dataReader[field.ColumnOrdinal]);

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
//	(Keep clean code rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
