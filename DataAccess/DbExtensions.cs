using System;
using System.Data;
using System.Data.Common;

namespace DbParallel.DataAccess
{
	public static partial class DbExtensions
	{
		internal static bool IsNullable(this Type type)
		{
			return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
		}

		internal static Type TryUnderlyingType(this Type type)
		{
			return type.IsNullable() ? Nullable.GetUnderlyingType(type) : type;
		}

		internal static bool CanMapToDbType(this Type type)
		{
			if (type.IsValueType)
				return true;
			if (type == typeof(string))
				return true;
			return false;
		}

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
					return (T)Convert.ChangeType(dbValue, typeof(T).TryUnderlyingType());
				}
			}
		}

		public static T Field<T>(this DbDataReader reader, string columnName)
		{
			return TryConvert<T>(reader[columnName]);
		}

		public static T Field<T>(this DbDataReader reader, int ordinal)
		{
			return TryConvert<T>(reader[ordinal]);
		}

		public static T Parameter<T>(this DbCommand cmd, string parameterName)
		{
			return TryConvert<T>(cmd.Parameters[parameterName].Value);
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

		public static DbParameter SetValue(this DbParameter dbParameter, object oValue)
		{
			dbParameter.Value = (oValue == null) ? DBNull.Value : oValue;
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
