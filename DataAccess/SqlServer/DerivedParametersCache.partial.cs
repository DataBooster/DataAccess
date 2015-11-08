using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DbParallel.DataAccess
{
	static partial class DerivedParametersCache
	{
		static partial void SqlDeriveParameters(DbCommand dbCmd, ref bool processed)
		{
			if (processed)
				return;

			SqlCommand sqlCmd = dbCmd as SqlCommand;

			if (sqlCmd != null)
			{
				SqlCommandBuilder.DeriveParameters(sqlCmd);

				int cntParams = sqlCmd.Parameters.Count;

				for (int i = 0; i < cntParams; i++)
					ResolveSqlTypeName(sqlCmd.Parameters[i]);

				processed = true;
			}
		}

		static private void ResolveSqlTypeName(SqlParameter sqlParameter)
		{
			string typeName = sqlParameter.TypeName;

			if (string.IsNullOrEmpty(typeName) || sqlParameter.SqlDbType != SqlDbType.Structured)
				return;

			for (int dots = 0, i = typeName.Length - 1; i >= 0; i--)
				if (typeName[i] == '.')
					if (++dots > 1)		// DatabaseName.SchemaName.TypeName
					{
						sqlParameter.TypeName = string.Empty;
						break;
					}
		}

		static partial void SqlOmitUnspecifiedInputParameters(DbCommand dbCmd, ref bool processed)
		{
			if (processed)
				return;

			SqlCommand sqlCmd = dbCmd as SqlCommand;

			if (sqlCmd != null && sqlCmd.CommandType == CommandType.StoredProcedure)
			{
				foreach (SqlParameter sqlParameter in sqlCmd.Parameters)
					if (sqlParameter.Value == null && (sqlParameter.Direction == ParameterDirection.InputOutput || sqlParameter.Direction == ParameterDirection.Output))
						sqlParameter.Value = DBNull.Value;

				processed = true;
			}
		}

		static partial void SqlAdaptParameterValueStringToBinary(DbParameter dbParameter, string specifiedParameterValue, ref bool processed)
		{
			if (processed)
				return;

			SqlParameter sqlParameter = dbParameter as SqlParameter;

			if (sqlParameter != null && sqlParameter.DbType == DbType.Binary)
			{
				dbParameter.Value = specifiedParameterValue.ToBytes();
				processed = true;
			}
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
//	Created Date:		2014-12-23
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
