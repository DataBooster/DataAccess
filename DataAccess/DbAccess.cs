using System;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	public partial class DbAccess : IDisposable
	{
		private const int _MaxRetryCount = 2;
		private DbConnection _Connection;

		public DbConnection Connection { get { return _Connection; } }

		public DbAccess(DbProviderFactory dbProviderFactory, string connectionString)
		{
			_Connection = dbProviderFactory.CreateConnection();
			_Connection.ConnectionString = connectionString;
			_Connection.Open();
		}

		public DbAccess(string providerName, string connectionString)
			: this(DbProviderFactories.GetFactory(providerName), connectionString)
		{
		}

		public DbAccess(ConnectionStringSettings connSetting)
			: this(DbProviderFactories.GetFactory(connSetting.ProviderName), connSetting.ConnectionString)
		{
		}

		public DbAccess(string connectionStringKey)
			: this(ConfigurationManager.ConnectionStrings[connectionStringKey])
		{
		}

		private DbCommand CreateCommand(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder)
		{
			DbCommand dbCommand = _Connection.CreateCommand();
			dbCommand.CommandType = commandType;
			dbCommand.CommandText = commandText;

			if (commandTimeout > 0)
				dbCommand.CommandTimeout = commandTimeout;

			if (parametersBuilder != null)
				parametersBuilder(new DbParameterBuilder(dbCommand));

			return dbCommand;
		}

		partial void OnOracleConnectionLoss(Exception dbException, ref bool canRetry);
		private bool OnConnectionLoss(Exception dbException)
		{
			bool canRetry = false;
			OnOracleConnectionLoss(dbException, ref canRetry);
			return canRetry;
		}

		private DbDataReader CreateReader(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder)
		{
			for (int retry = 0; ; retry++)
			{
				try
				{
					return CreateCommand(commandText, commandTimeout, commandType, parametersBuilder).ExecuteReader();
				}
				catch (Exception e)
				{
					if (retry < _MaxRetryCount && OnConnectionLoss(e))
						ReConnect();
					else
						throw;
				}
			}
		}

		public void ExecuteReader(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder, Action<DbDataReader> dataReader)
		{
			using (DbDataReader reader = CreateReader(commandText, commandTimeout, commandType, parametersBuilder))
			{
				if (dataReader != null)
					while (reader.Read())
						dataReader(reader);
			}
		}

		public void ExecuteReader(string commandText, Action<DbParameterBuilder> parametersBuilder, Action<DbDataReader> dataReader)
		{
			ExecuteReader(commandText, 0, CommandType.StoredProcedure, parametersBuilder, dataReader);
		}

		public void ExecuteReader(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder, Action<DbDataReader, int> dataReaders)
		{
			using (DbDataReader reader = CreateReader(commandText, commandTimeout, commandType, parametersBuilder))
			{
				if (dataReaders != null)
				{
					int resultSet = 0;

					do
					{
						while (reader.Read())
							dataReaders(reader, resultSet);

						resultSet++;
					} while (reader.NextResult());
				}
			}
		}

		public void ExecuteReader(string commandText, Action<DbParameterBuilder> parametersBuilder, Action<DbDataReader, int> dataReaders)
		{
			ExecuteReader(commandText, 0, CommandType.StoredProcedure, parametersBuilder, dataReaders);
		}

		public void ExecuteReader<T>(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder,
			Action<DbFieldMap<T>> resultMap, Action<T> readEntity) where T : new()
		{
			using (DbDataReader reader = CreateReader(commandText, commandTimeout, commandType, parametersBuilder))
			{
				if (readEntity != null)
				{
					DbFieldMap<T> map = new DbFieldMap<T>();

					if (resultMap == null)
						map.AddAllPropertiesOrFields();
					else
						resultMap(map);

					while (reader.Read())
						readEntity(map.ReadNew(reader));
				}
			}
		}

		public void ExecuteReader<T>(string commandText, Action<DbParameterBuilder> parametersBuilder,
			Action<DbFieldMap<T>> resultMap, Action<T> readEntity) where T : new()
		{
			ExecuteReader<T>(commandText, 0, CommandType.StoredProcedure, parametersBuilder, resultMap, readEntity);
		}

		public void ExecuteReader<T>(string commandText, Action<DbParameterBuilder> parametersBuilder, Action<T> readEntity) where T : new()
		{
			ExecuteReader<T>(commandText, 0, CommandType.StoredProcedure, parametersBuilder, null, readEntity);
		}

		public IEnumerable<T> ExecuteReader<T>(string commandText, int commandTimeout, CommandType commandType,
			Action<DbParameterBuilder> parametersBuilder, Action<DbFieldMap<T>> resultMap = null) where T : new()
		{
			using (DbDataReader reader = CreateReader(commandText, commandTimeout, commandType, parametersBuilder))
			{
				DbFieldMap<T> map = new DbFieldMap<T>();

				if (resultMap == null)
					map.AddAllPropertiesOrFields();
				else
					resultMap(map);

				while (reader.Read())
					yield return map.ReadNew(reader);
			}
		}

		public IEnumerable<T> ExecuteReader<T>(string commandText, Action<DbParameterBuilder> parametersBuilder,
			Action<DbFieldMap<T>> resultMap = null) where T : new()
		{
			return ExecuteReader<T>(commandText, 0, CommandType.StoredProcedure, parametersBuilder, resultMap);
		}

		public int ExecuteNonQuery(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder)
		{
			int nAffectedRows = 0;

			for (int retry = 0; ; retry++)
			{
				try
				{
					nAffectedRows = CreateCommand(commandText, commandTimeout, commandType, parametersBuilder).ExecuteNonQuery();
					break;
				}
				catch (Exception e)
				{
					if (retry < _MaxRetryCount && OnConnectionLoss(e))
						ReConnect();
					else
						throw;
				}
			}

			return nAffectedRows;
		}

		public int ExecuteNonQuery(string commandText, Action<DbParameterBuilder> parametersBuilder = null)
		{
			return ExecuteNonQuery(commandText, 0, CommandType.StoredProcedure, parametersBuilder);
		}

		private void ReConnect()
		{
			if (_Connection != null)
				if (_Connection.State != ConnectionState.Closed)
				{
					_Connection.Close();
					_Connection.Open();
				}
		}

		#region IDisposable Members
		public void Dispose()
		{
			if (_Connection != null)
			{
				if (_Connection.State != ConnectionState.Closed)
					_Connection.Close();
				_Connection = null;
			}
		}
		#endregion
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
