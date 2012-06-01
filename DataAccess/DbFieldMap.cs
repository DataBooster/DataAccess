using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data.Common;
using System.Linq.Expressions;

namespace DbParallel.DataAccess
{
	public class DbFieldMap<T> where T : new()
	{
		class PropertyOrField
		{
			public string ColumnName { get; set; }
			public int ColumnOrdinal { get; set; }
			private PropertyInfo _PropertyInfo;
			private FieldInfo _FieldInfo;
			private Type _ValueType;

			public MemberInfo MemberInfo
			{
				get
				{
					if (_PropertyInfo != null)
						return _PropertyInfo;
					else
						return _FieldInfo;
				}
				set
				{
					_PropertyInfo = value as PropertyInfo;

					if (_PropertyInfo != null)
						_ValueType = _PropertyInfo.PropertyType.TryUnderlyingType();
					else
					{
						_FieldInfo = value as FieldInfo;

						if (_FieldInfo != null)
							_ValueType = _FieldInfo.FieldType.TryUnderlyingType();
					}

					if (_ValueType.CanMapToDbType() == false)
						throw new ApplicationException("The (Underlying)Type of Property Or Field must be a Value Type.");
				}
			}

			public void SetValue(object objEntity, object dbValue)
			{
				if (Convert.IsDBNull(dbValue))
					return;

				if (_PropertyInfo != null)
					_PropertyInfo.SetValue(objEntity, Convert.ChangeType(dbValue, _ValueType), null);
				else if (_FieldInfo != null)
					_FieldInfo.SetValue(objEntity, Convert.ChangeType(dbValue, _ValueType));
			}
		}

		private List<PropertyOrField> _FieldList;
		private ulong _RowCount;

		public DbFieldMap()
		{
			_FieldList = new List<PropertyOrField>();
			_RowCount = 0;
		}

		private void MapColumns(DbDataReader dataReader)
		{
			PropertyOrField field;

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
			MemberExpression memberExpression = fieldExpr.Body as MemberExpression;

			if (memberExpression == null)
			{
				UnaryExpression unaryExpression = fieldExpr.Body as UnaryExpression;

				if (unaryExpression != null)
					memberExpression = unaryExpression.Operand as MemberExpression;
			}

			if (memberExpression == null)
				throw new ApplicationException("Expression must be a Property or a Field.");

			_FieldList.Add(new PropertyOrField() { ColumnName = columnName, MemberInfo = memberExpression.Member });

			return this;
		}

		internal void AddAllPropertiesOrFields()
		{
			Type type = typeof(T);

			foreach (PropertyInfo p in type.GetProperties())
			{
				if (p.CanWrite && p.CanRead && p.PropertyType.TryUnderlyingType().CanMapToDbType())
					_FieldList.Add(new PropertyOrField() { ColumnName = p.Name, MemberInfo = p });
			}

			foreach (FieldInfo f in type.GetFields())
			{
				if (f.IsInitOnly == false && f.FieldType.TryUnderlyingType().CanMapToDbType())
					_FieldList.Add(new PropertyOrField() { ColumnName = f.Name, MemberInfo = f });
			}
		}

		internal T ReadNew(DbDataReader dataReader)
		{
			T entity = new T();

			if (_RowCount == 0L)
				MapColumns(dataReader);

			foreach (PropertyOrField field in _FieldList)
				field.SetValue(entity, dataReader[field.ColumnOrdinal]);

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
//	Primary Host:		http://dbParallel.codeplex.com
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep clean code rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
