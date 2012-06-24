#if ORACLE
using System;
using System.Data;
#if DATADIRECT
using DDTek.Oracle;
#else // ODP.NET
using Oracle.DataAccess.Client;
#endif

namespace DbParallel.DataAccess.Booster.Oracle
{
	public class OracleLauncher : DbLauncher
	{
		public OracleLauncher(string connectionString, string storedProcedure, Action<DbParameterBuilder> parametersBuilder,
			int multipleRockets = _DefaultMultipleRockets, int bulkSize = _DefaultBulkSize, int commandTimeout = _CommandTimeout)
		{
			int[] associativeArrayParameterIds;

			if (multipleRockets < _MinMultipleRockets)
				multipleRockets = _MinMultipleRockets;

			if (bulkSize < _MinBulkSize)
				bulkSize = _MinBulkSize;

			OracleCommand dbCommand = CreateCommand(connectionString, storedProcedure, parametersBuilder, commandTimeout);
			associativeArrayParameterIds = OracleRocket.SearchAssociativeArrayParameters(dbCommand.Parameters);
			_FillingRocket = new OracleRocket(dbCommand, associativeArrayParameterIds, bulkSize);

			for (int i = 1; i < multipleRockets; i++)
			{
				dbCommand = CreateCommand(connectionString, storedProcedure, parametersBuilder, commandTimeout);
				_FreeQueue.Add(new OracleRocket(dbCommand, associativeArrayParameterIds, bulkSize));
			}
		}

		private static OracleCommand CreateCommand(string connectionString, string storedProcedure,
			Action<DbParameterBuilder> parametersBuilder, int commandTimeout)
		{
			OracleConnection dbConnection = new OracleConnection(connectionString);

			OracleCommand dbCommand = dbConnection.CreateCommand();
			dbCommand.CommandType = CommandType.StoredProcedure;
			dbCommand.CommandText = storedProcedure;

			if (commandTimeout > 0)
				dbCommand.CommandTimeout = commandTimeout;

			parametersBuilder(new DbParameterBuilder(dbCommand));

			return dbCommand;
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
//	Created Date:		2012-05-18
//	Primary Host:		http://databooster.codeplex.com
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep clean code rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
