using System;
using System.Data.Common;
using System.Configuration;
using System.Data.SqlClient;

namespace DbParallel.DataAccess.Booster.SqlServer
{
	public class SqlLauncher : DbLauncher
	{
		public SqlLauncher(DbProviderFactory dbProviderFactory, string connectionString, string destinationTableName, Action<SqlBulkCopyColumnMappingCollection> columnMappings = null,
			int multipleRockets = _DefaultMultipleRockets, int bulkSize = _DefaultBulkSize, int commandTimeout = _CommandTimeout)
		{
			if (multipleRockets < _MinMultipleRockets)
				multipleRockets = _MinMultipleRockets;

			if (bulkSize < _MinBulkSize)
				bulkSize = _MinBulkSize;

			SqlConnection dbConnection;
			SqlBulkCopy bulkCopy = CreateBulkCopy(dbProviderFactory, connectionString, out dbConnection, destinationTableName, columnMappings, commandTimeout);
			_FillingRocket = new SqlRocket(bulkCopy, dbConnection, bulkSize);

			for (int i = 1; i < multipleRockets; i++)
			{
				bulkCopy = CreateBulkCopy(dbProviderFactory, connectionString, out dbConnection, destinationTableName, columnMappings, commandTimeout);
				_FreeQueue.Add(new SqlRocket(bulkCopy, dbConnection, bulkSize));
			}
		}

		public SqlLauncher(string providerName, string connectionString, string destinationTableName, Action<SqlBulkCopyColumnMappingCollection> columnMappings = null,
			int multipleRockets = _DefaultMultipleRockets, int bulkSize = _DefaultBulkSize, int commandTimeout = _CommandTimeout)
			: this(DbProviderFactories.GetFactory(providerName), connectionString,
			destinationTableName, columnMappings, multipleRockets, bulkSize, commandTimeout)
		{
		}

		public SqlLauncher(ConnectionStringSettings connSetting, string destinationTableName, Action<SqlBulkCopyColumnMappingCollection> columnMappings = null,
			int multipleRockets = _DefaultMultipleRockets, int bulkSize = _DefaultBulkSize, int commandTimeout = _CommandTimeout)
			: this(DbProviderFactories.GetFactory(connSetting.ProviderName), connSetting.ConnectionString,
			destinationTableName, columnMappings, multipleRockets, bulkSize, commandTimeout)
		{
		}

		public SqlLauncher(string connectionStringKey, string destinationTableName, Action<SqlBulkCopyColumnMappingCollection> columnMappings = null,
			int multipleRockets = _DefaultMultipleRockets, int bulkSize = _DefaultBulkSize, int commandTimeout = _CommandTimeout)
			: this(ConfigurationManager.ConnectionStrings[connectionStringKey],
			destinationTableName, columnMappings, multipleRockets, bulkSize, commandTimeout)
		{
		}

		private static SqlBulkCopy CreateBulkCopy(DbProviderFactory dbProviderFactory, string connectionString, out SqlConnection dbConnection,
			string destinationTableName, Action<SqlBulkCopyColumnMappingCollection> mapColumns, int commandTimeout)
		{
			dbConnection = dbProviderFactory.CreateConnection() as SqlConnection;
			dbConnection.ConnectionString = connectionString;

			SqlBulkCopy bulkCopy = new SqlBulkCopy(dbConnection);

			bulkCopy.DestinationTableName = destinationTableName;

			if (commandTimeout > 0)
				bulkCopy.BulkCopyTimeout = commandTimeout;

			if (mapColumns != null)
				mapColumns(bulkCopy.ColumnMappings);

			return bulkCopy;
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
//	Created Date:		2012-06-09
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
