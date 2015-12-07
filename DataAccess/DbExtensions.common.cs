using System;
using System.Data.Linq;
using System.Reflection;
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
			if (type == typeof(byte[]))
				return true;
			return false;
		}
		#endregion

		internal static T TryConvert<T>(object dbValue)
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

		internal static IEnumerable<ColumnMemberInfo> AllPropertiesOrFields(this Type type)
		{
			foreach (FieldInfo f in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
				if (f.IsInitOnly == false && f.FieldType.GetNonNullableType().CanMapToDbType())
					yield return new ColumnMemberInfo(f.Name, f);

			foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				if (p.CanWrite && p.CanRead && p.PropertyType.GetNonNullableType().CanMapToDbType())
					yield return new ColumnMemberInfo(p.Name, p);
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
