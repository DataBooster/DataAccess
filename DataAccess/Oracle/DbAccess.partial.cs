#if ORACLE
using System;
using System.Linq;
using System.Data;
using System.Data.Common;

#if DATADIRECT
using DDTek.Oracle;
#elif ODP_NET	// ODP.NET
using Oracle.DataAccess.Client;
#else			// ODP.NET.Managed
using Oracle.ManagedDataAccess.Client;
#endif

namespace DbParallel.DataAccess
{
	partial class DbAccess
	{
		partial void OnOracleContextLost(Exception dbException, ref RetryAction retryAction, ref bool processed)
		{
			if (processed)
				return;

			if (_Connection is OracleConnection)
			{
				OracleException e = dbException as OracleException;

				if (e == null)
					retryAction = RetryAction.None;
				else
					switch (e.Number)
					{
						case 2396:	// ORA-02396: exceeded maximum idle time, please connect again
						case 3113:	// ORA-03113: end-of-file on communication channel
						case 3135:	// ORA-03135: connection lost contact
						case 4068:	// ORA-04068: existing state of packagesstringstringstring has been discarded
							retryAction = RetryAction.Reconnect;
							break;
						case 6550:
							retryAction = (e.Errors.Count == 1 && e.Message.Contains("PLS-00306: ")) ? RetryAction.RefreshParameters : RetryAction.None;
							break;
						// To add other cases
						default:
							retryAction = RetryAction.None;
							break;
					}

				processed = true;
			}
		}

		// DataDirect Oracle Data Provider Connection String Options support: [ParameterMode = ANSI | BindByOrdinal | BindByName]

#if (ODP_NET || ODP_NET_MANAGED)		// ODP.NET
		/*
		partial void OnOracleCommandCreating(DbCommand dbCmd, ref bool processed)
		{
			if (processed)
				return;

			OracleCommand oraCmd = dbCmd as OracleCommand;

			if (oraCmd != null)
			{
				oraCmd.BindByName = true;
				processed = true;
			}
		}
		*/

		partial void OnOracleReaderExecuting(DbCommand dbCmd, int resultSetCnt/* = 1 */, ref bool processed)
		{
			if (processed)
				return;

			OracleCommand oraCmd = dbCmd as OracleCommand;

			if (oraCmd != null)
			{
				if (oraCmd.CommandType == CommandType.StoredProcedure)
				{
					int cntRefCursorParam = oraCmd.Parameters.OfType<OracleParameter>().Count(p => p.OracleDbType == OracleDbType.RefCursor && p.Direction != ParameterDirection.Input);

					if (cntRefCursorParam < resultSetCnt)
						if (oraCmd.BindByName || _AutoDeriveRefCursorParameters)
							DerivedParametersCache.DeriveParameters(dbCmd, null, false);
						else
						{
							for (; cntRefCursorParam < resultSetCnt; cntRefCursorParam++)
							{
								OracleParameter paramRefCursor = oraCmd.CreateParameter();
								paramRefCursor.OracleDbType = OracleDbType.RefCursor;
								paramRefCursor.Direction = ParameterDirection.Output;
								paramRefCursor.Value = DBNull.Value;

								oraCmd.Parameters.Add(paramRefCursor);
							}
						}
				}

				processed = true;
			}
		}
#endif
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
//	Original Host:		http://dbParallel.codeplex.com
//	Primary Host:		http://DataBooster.codeplex.com
//	Updated Host:		https://github.com/DataBooster/DataAccess
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
