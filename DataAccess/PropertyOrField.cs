using System;
using System.Reflection;

namespace DbParallel.DataAccess
{
	public class PropertyOrField
	{
		private PropertyInfo _PropertyInfo;
		private FieldInfo _FieldInfo;

		private Type _DataType;
		public Type DataType { get { return _DataType; } }

		public PropertyOrField(MemberInfo memberInfo)
		{
			_PropertyInfo = memberInfo as PropertyInfo;

			if (_PropertyInfo != null)
				_DataType = _PropertyInfo.PropertyType.TryUnderlyingType();
			else
			{
				_FieldInfo = memberInfo as FieldInfo;

				if (_FieldInfo != null)
					_DataType = _FieldInfo.FieldType.TryUnderlyingType();
			}
		}

		public void SetValue(object objEntity, object dbValue)
		{
			if (Convert.IsDBNull(dbValue))
				return;

			if (_PropertyInfo != null)
				_PropertyInfo.SetValue(objEntity, Convert.ChangeType(dbValue, _DataType), null);
			else if (_FieldInfo != null)
				_FieldInfo.SetValue(objEntity, Convert.ChangeType(dbValue, _DataType));
		}

		public object GetValue(object objEntity)
		{
			if (_PropertyInfo != null)
				return _PropertyInfo.GetValue(objEntity, null);
			else if (_FieldInfo != null)
				return _FieldInfo.GetValue(objEntity);
			else
				return null;
		}

		public object ConstructNestedMember(object containerObject)
		{
			if (_DataType != null && containerObject != null)
			{
				object memberObject = GetValue(containerObject);

				if (_DataType.IsClass && memberObject == null)
				{
					ConstructorInfo memberConstructor = _DataType.GetConstructor(Type.EmptyTypes);

					if (memberConstructor != null && memberConstructor.IsPublic)
					{
						try
						{
							memberObject = memberConstructor.Invoke(null);

							if (_PropertyInfo != null)
								_PropertyInfo.SetValue(containerObject, memberObject, null);
							else if (_FieldInfo != null)
								_FieldInfo.SetValue(containerObject, memberObject);
						}
						catch
						{
							memberObject = null;
						}
					}
				}

				return memberObject;
			}

			return null;
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
//	Created Date:		2014-09-12
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
