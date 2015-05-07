using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DbParallel.DataAccess
{
	static partial class DerivedParametersCache
	{
		static partial void SqlDeriveParameters(DbCommand dbCmd)
		{
			SqlCommand sqlCmd = dbCmd as SqlCommand;

			if (sqlCmd != null)
			{
				SqlCommandBuilder.DeriveParameters(sqlCmd);

				int cntParams = sqlCmd.Parameters.Count;

				for (int i = 0; i < cntParams; i++)
					ResolveSqlTypeName(sqlCmd.Parameters[i]);
			}
		}

		static private void ResolveSqlTypeName(SqlParameter sqlParameter)
		{
			string typeName = sqlParameter.TypeName;

			if (string.IsNullOrEmpty(typeName) || sqlParameter.SqlDbType != SqlDbType.Structured)
				return;

			int countParts = 0, inBrackets = 0;
			bool inQuotations = false, inEscape = false;
			char lastChar = char.MinValue;

			for (int i = typeName.Length - 1; i >= 0; i--)
			{
				switch (typeName[i])
				{
					case ']':
						if (lastChar == ']')
							inEscape = !inEscape;

						if (inEscape)
							inBrackets--;
						else
							inBrackets++;
						break;
					case '[':
						inBrackets--;
						break;
					case '"':
						inQuotations = !inQuotations;
						break;
					case '.':
						if (inBrackets <= 0 && inQuotations == false)
						{
							countParts++;

							if (countParts == 2)	// DatabaseName.SchemaName.TypeName
							{
								sqlParameter.TypeName = typeName.Substring(i + 1);
								return;
							}
						}
						break;
				}

				lastChar = typeName[i];

				if (lastChar != ']' && inEscape)
					inEscape = false;
			}
		}

		static partial void SqlOmitUnspecifiedInputParameters(DbCommand dbCmd)
		{
			SqlCommand sqlCmd = dbCmd as SqlCommand;

			if (sqlCmd != null && sqlCmd.CommandType == CommandType.StoredProcedure)
			{
				foreach (SqlParameter sqlParameter in sqlCmd.Parameters)
					if (sqlParameter.Value == null && (sqlParameter.Direction == ParameterDirection.InputOutput || sqlParameter.Direction == ParameterDirection.Output))
						sqlParameter.Value = DBNull.Value;
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
