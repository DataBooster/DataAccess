using System;
using System.Reflection;
using System.Linq.Expressions;

namespace DbParallel.DataAccess
{
	public class ColumnMemberInfo
	{
		private string _ColumnName;
		public string ColumnName { get { return _ColumnName; } }

		public int ColumnOrdinal { get; set; }

		private PropertyOrField[] _DeepMemberRoute;
		public PropertyOrField[] DeepMemberRoute { get { return _DeepMemberRoute; } }

		public ColumnMemberInfo(string columnName, LambdaExpression fieldExpr)
		{
			_ColumnName = columnName;
			_DeepMemberRoute = fieldExpr.GetDeepMemberRoute();

			int depth = _DeepMemberRoute.Length;

			if (depth == 0 || _DeepMemberRoute[depth - 1].DataType.CanMapToDbType() == false)
				throw new ApplicationException("The (Underlying)Type of end Property Or Field must be a Value Type.");
		}

		public ColumnMemberInfo(string columnName, PropertyInfo propertyInfo)
		{
			_ColumnName = columnName;
			_DeepMemberRoute = new PropertyOrField[] { new PropertyOrField(propertyInfo) };
		}

		public ColumnMemberInfo(string columnName, FieldInfo fieldInfo)
		{
			_ColumnName = columnName;
			_DeepMemberRoute = new PropertyOrField[] { new PropertyOrField(fieldInfo) };
		}

		public bool SetValue(object rootObject, object dbValue)
		{
			return _DeepMemberRoute.SetDeepMemberValue(rootObject, dbValue);
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2014 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2014-09-15
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
