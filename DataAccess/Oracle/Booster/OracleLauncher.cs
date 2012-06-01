#if ORACLE
using System;
using System.Data;
using System.Data.Common;
using System.Collections.Concurrent;
using System.Configuration;
using DDTek.Oracle;

namespace DbParallel.DataAccess.Oracle.Booster
{
	public class OracleLauncher : IDisposable
	{
		private const int _MinMultipleRockets = 3;
		private const int _MinBulkSize = 1000;
		private const int _DefaultMultipleRockets = 8;
		private const int _DefaultBulkSize = 1000000;
		private const int _CommandTimeout = 3600;

		private readonly BlockingCollection<OracleRocket> _FreeQueue;
		private OracleRocket _FillingRocket;
		private object _FillingLock;
		private ParallelExecuteWaitHandle _ExecutingHandle;

		private bool _Disposed = false;

		public OracleLauncher(DbProviderFactory dbProviderFactory, string connectionString, string storedProcedure, Action<DbParameterBuilder> parametersBuilder,
			int multipleRockets = _DefaultMultipleRockets, int bulkSize = _DefaultBulkSize, int commandTimeout = _CommandTimeout)
		{
			int[] associativeArrayParameterIds;

			if (multipleRockets < _MinMultipleRockets)
				multipleRockets = _MinMultipleRockets;

			if (bulkSize < _MinBulkSize)
				bulkSize = _MinBulkSize;

			OracleCommand dbCommand = CreateCommand(dbProviderFactory, connectionString, storedProcedure, parametersBuilder, commandTimeout);
			associativeArrayParameterIds = OracleRocket.SearchAssociativeArrayParameters(dbCommand.Parameters);
			_FillingRocket = new OracleRocket(dbCommand, associativeArrayParameterIds, bulkSize);
			_FreeQueue = new BlockingCollection<OracleRocket>();

			for (int i = 1; i < multipleRockets; i++)
			{
				dbCommand = CreateCommand(dbProviderFactory, connectionString, storedProcedure, parametersBuilder, commandTimeout);
				_FreeQueue.Add(new OracleRocket(dbCommand, associativeArrayParameterIds, bulkSize));
			}

			_FillingLock = new object();
			_ExecutingHandle = new ParallelExecuteWaitHandle();
		}

		public OracleLauncher(string providerName, string connectionString, string storedProcedure, Action<DbParameterBuilder> parametersBuilder,
			int multipleRockets = _DefaultMultipleRockets, int bulkSize = _DefaultBulkSize, int commandTimeout = _CommandTimeout)
			: this(DbProviderFactories.GetFactory(providerName), connectionString,
			storedProcedure, parametersBuilder, multipleRockets, bulkSize, commandTimeout)
		{
		}

		public OracleLauncher(ConnectionStringSettings connSetting, string storedProcedure, Action<DbParameterBuilder> parametersBuilder,
			int multipleRockets = _DefaultMultipleRockets, int bulkSize = _DefaultBulkSize, int commandTimeout = _CommandTimeout)
			: this(DbProviderFactories.GetFactory(connSetting.ProviderName), connSetting.ConnectionString,
			storedProcedure, parametersBuilder, multipleRockets, bulkSize, commandTimeout)
		{
		}

		public OracleLauncher(string connectionStringKey, string storedProcedure, Action<DbParameterBuilder> parametersBuilder,
			int multipleRockets = _DefaultMultipleRockets, int bulkSize = _DefaultBulkSize, int commandTimeout = _CommandTimeout)
			: this(ConfigurationManager.ConnectionStrings[connectionStringKey],
			storedProcedure, parametersBuilder, multipleRockets, bulkSize, commandTimeout)
		{
		}

		private static OracleCommand CreateCommand(DbProviderFactory dbProviderFactory, string connectionString,
			string storedProcedure, Action<DbParameterBuilder> parametersBuilder, int commandTimeout)
		{
			OracleConnection dbConnection = dbProviderFactory.CreateConnection() as OracleConnection;
			dbConnection.ConnectionString = connectionString;

			OracleCommand dbCommand = dbConnection.CreateCommand();
			dbCommand.CommandType = CommandType.StoredProcedure;
			dbCommand.CommandText = storedProcedure;

			if (commandTimeout > 0)
				dbCommand.CommandTimeout = commandTimeout;

			parametersBuilder(new DbParameterBuilder(dbCommand));

			return dbCommand;
		}

		public void AddRow(params object[] values)
		{
			lock (_FillingLock)
			{
				if (_FillingRocket.AddRow(values))
				{
					_ExecutingHandle.StartNewTask(LaunchRocket, _FillingRocket);
					_FillingRocket = _FreeQueue.Take();
				}
			}
		}

		private void LaunchRocket(OracleRocket rocket)
		{
			rocket.Launch();
			_FreeQueue.Add(rocket);
		}

		public void CompleteAdding()
		{
			lock (_FillingLock)
			{
				_FillingRocket.Launch();
				_ExecutingHandle.Wait();
			}
		}

		public void Dispose()
		{
			if (_Disposed == false)
			{
				CompleteAdding();

				foreach (OracleRocket rocket in _FreeQueue)
					rocket.Dispose();

				if (_FillingRocket != null)
					_FillingRocket.Dispose();

				_ExecutingHandle.Dispose();

				_Disposed = true;
			}
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
