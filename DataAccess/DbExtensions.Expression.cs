using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace DbParallel.DataAccess
{
	public static partial class DbExtensions
	{
		internal static MemberExpression GetMemberExpression(this Expression expression)
		{
			MemberExpression memberExpression = expression as MemberExpression;

			if (memberExpression == null)
			{
				LambdaExpression lambdaExpression = expression as LambdaExpression;

				if (lambdaExpression != null)
				{
					memberExpression = lambdaExpression.Body as MemberExpression;

					if (memberExpression == null)
					{
						UnaryExpression unaryExpression = lambdaExpression.Body as UnaryExpression;

						if (unaryExpression != null)
							memberExpression = unaryExpression.Operand as MemberExpression;
					}
				}
			}

			return memberExpression;
		}
		
		internal static PropertyOrField[] GetNestedMemberChain(this Expression exprMemberPath)
		{
			Stack<PropertyOrField> memberChain = new Stack<PropertyOrField>();

			for (MemberExpression memberExpression = exprMemberPath.GetMemberExpression(); memberExpression != null; memberExpression = memberExpression.Expression.GetMemberExpression())
				if (memberExpression.Member is PropertyInfo || memberExpression.Member is FieldInfo)
					memberChain.Push(new PropertyOrField(memberExpression.Member));

			return memberChain.ToArray();
		}

		internal static bool SetDeepMemberValue(this PropertyOrField[] memberChain, object rootObject, object dbValue)
		{
			int depth, midMembers = memberChain.Length - 1;
			object memberObject = rootObject;

			for (depth = 0; depth < midMembers && memberObject != null; depth++)
				memberObject = memberChain[depth].ConstructNestedMember(memberObject);

			if (memberObject != null && depth == midMembers)
			{
				memberChain[depth].SetValue(memberObject, dbValue);
				return true;
			}
			else
				return false;
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
