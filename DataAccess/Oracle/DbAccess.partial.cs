#if ORACLE
using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
#if DATADIRECT
using DDTek.Oracle;
#else // ODP.NET
using Oracle.DataAccess.Client;
#endif

namespace DbParallel.DataAccess
{
	public partial class DbAccess
	{
		partial void OnOracleConnectionLost(Exception dbException, ref bool canRetry)
		{
			if (_Connection is OracleConnection)
			{
				OracleException e = dbException as OracleException;

				if (e == null)
					canRetry = false;
				else
					switch (e.Number)
					{
						case 3113:
						case 4068: canRetry = true; break;
						// To add other cases
						default: canRetry = false; break;
					}
			}
		}

#if ODP_NET		// ODP.NET
		partial void OnOracleReaderExecuting(DbCommand dbCmd)
		{
			OracleCommand oraCmd = dbCmd as OracleCommand;

			if (oraCmd != null && oraCmd.CommandType == CommandType.StoredProcedure)
			{
				OracleParameter paramRefCursor = oraCmd.Parameters.Cast<OracleParameter>().LastOrDefault(p => p.OracleDbType == OracleDbType.RefCursor && p.Direction != ParameterDirection.Input);

				if (paramRefCursor == null)
					if (oraCmd.BindByName)
					{
						OracleParameterCollection paramCollection = oraCmd.Parameters;
						IEnumerable<string> explicitParamNames = paramCollection.Cast<OracleParameter>().Select(p => p.ParameterName);

						foreach (OracleParameter derivedParam in DeriveParameters(oraCmd.CommandText))
							if (!explicitParamNames.Contains(derivedParam.ParameterName, StringComparer.OrdinalIgnoreCase))
								paramCollection.Add(derivedParam);
					}
					else
					{
						paramRefCursor = oraCmd.CreateParameter();
						paramRefCursor.OracleDbType = OracleDbType.RefCursor;
						paramRefCursor.Direction = ParameterDirection.Output;
						paramRefCursor.Value = DBNull.Value;

						oraCmd.Parameters.Add(paramRefCursor);
					}
			}
		}
#endif

		private OracleParameter[] DeriveParameters(string storedProcedure)
		{
			OracleCommand oraCmd = _Connection.CreateCommand() as OracleCommand;
			oraCmd.CommandType = CommandType.StoredProcedure;
			oraCmd.CommandText = storedProcedure;

			OracleCommandBuilder.DeriveParameters(oraCmd);

			OracleParameter[] derivedParams = oraCmd.Parameters.Cast<OracleParameter>().ToArray();
			oraCmd.Parameters.Clear();
			oraCmd = null;

			return derivedParams;
		}
	}
}
#endif

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
