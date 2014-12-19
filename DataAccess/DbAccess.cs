using System;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Dynamic;
using System.Linq;

namespace DbParallel.DataAccess
{
	public partial class DbAccess : IDisposable
	{
		private static CommandType _DefaultCommandType = CommandType.StoredProcedure;
		public static CommandType DefaultCommandType
		{
			get { return _DefaultCommandType; }
			set { _DefaultCommandType = value; }
		}

		private const int _MaxRetryCount = 2;
		private const int _IncreasingDelayRetry = 500;		// Increases 500 milliseconds delay time for every retry.

		private DbProviderFactory _ProviderFactory;
		private DbConnection _Connection;
		public DbConnection Connection
		{
			get
			{
				if (_Connection == null)
					throw new ObjectDisposedException("DbAccess");

				return _Connection;
			}
		}

		#region Constructors

		public DbAccess(DbProviderFactory dbProviderFactory, string connectionString)
		{
			_ProviderFactory = dbProviderFactory;

			_Connection = dbProviderFactory.CreateConnection();
			_Connection.ConnectionString = connectionString;
			_Connection.Open();

			_TransactionManager = new DbTransactionManager(_Connection);
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

		#endregion

		private DbCommand CreateCommand(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder)
		{
			if (_Connection == null)
				throw new ObjectDisposedException("DbAccess");

			DbCommand dbCommand = _Connection.CreateCommand();
			dbCommand.CommandType = commandType;
			dbCommand.CommandText = commandText;
			dbCommand.Transaction = _TransactionManager.Transaction;

			if (commandTimeout > 0)
				dbCommand.CommandTimeout = commandTimeout;

			if (parametersBuilder != null)
				parametersBuilder(new DbParameterBuilder(dbCommand));

			return dbCommand;
		}

		private DbDataReader CreateReader(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder, int resultSetCnt = 1)
		{
			for (int retry = 0; ; retry++)
			{
				try
				{
					DbCommand dbCmd = CreateCommand(commandText, commandTimeout, commandType, parametersBuilder);

					OnReaderExecuting(dbCmd, resultSetCnt);

					return dbCmd.ExecuteReader();
				}
				catch (Exception e)
				{
					if (retry < _MaxRetryCount && OnConnectionLost(e))
						ReConnect(retry);
					else
						throw;
				}
			}
		}

		#region ExecuteReader Overloads

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
			ExecuteReader(commandText, 0, _DefaultCommandType, parametersBuilder, dataReader);
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
			ExecuteReader(commandText, 0, _DefaultCommandType, parametersBuilder, dataReaders);
		}

		public void ExecuteReader<T>(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder,
			Action<DbFieldMap<T>> resultMap, Action<T> readEntity) where T : class, new()
		{
			using (DbDataReader reader = CreateReader(commandText, commandTimeout, commandType, parametersBuilder))
			{
				if (readEntity != null)
				{
					DbFieldMap<T> map = new DbFieldMap<T>();

					map.PrepareResultMap(resultMap);

					while (reader.Read())
						readEntity(map.ReadNew(reader));
				}
			}
		}

		public void ExecuteReader<T>(string commandText, Action<DbParameterBuilder> parametersBuilder,
			Action<DbFieldMap<T>> resultMap, Action<T> readEntity) where T : class, new()
		{
			ExecuteReader<T>(commandText, 0, _DefaultCommandType, parametersBuilder, resultMap, readEntity);
		}

		public void ExecuteReader<T>(string commandText, Action<DbParameterBuilder> parametersBuilder, Action<T> readEntity) where T : class, new()
		{
			ExecuteReader<T>(commandText, 0, _DefaultCommandType, parametersBuilder, null, readEntity);
		}

		public IEnumerable<T> ExecuteReader<T>(string commandText, int commandTimeout, CommandType commandType,
			Action<DbParameterBuilder> parametersBuilder, Action<DbFieldMap<T>> resultMap = null) where T : class, new()
		{
			using (DbDataReader reader = CreateReader(commandText, commandTimeout, commandType, parametersBuilder))
			{
				DbFieldMap<T> map = new DbFieldMap<T>();

				map.PrepareResultMap(resultMap);

				while (reader.Read())
					yield return map.ReadNew(reader);
			}
		}

		public IEnumerable<T> ExecuteReader<T>(string commandText, Action<DbParameterBuilder> parametersBuilder,
			Action<DbFieldMap<T>> resultMap = null) where T : class, new()
		{
			return ExecuteReader<T>(commandText, 0, _DefaultCommandType, parametersBuilder, resultMap);
		}

		#endregion

		#region ExecuteMultiReader Overloads

		public void ExecuteMultiReader(string commandText, int commandTimeout, CommandType commandType,
			Action<DbParameterBuilder> parametersBuilder, Action<DbMultiResultSet> multiResultSetMap)
		{
			DbMultiResultSet multiResultSet = new DbMultiResultSet();

			if (multiResultSetMap != null)
				multiResultSetMap(multiResultSet);

			using (DbDataReader reader = CreateReader(commandText, commandTimeout, commandType, parametersBuilder, multiResultSet.Count))
			{
				multiResultSet.ReadAll(reader);
			}
		}

		public void ExecuteMultiReader(string commandText, Action<DbParameterBuilder> parametersBuilder, Action<DbMultiResultSet> multiResultSetMap)
		{
			ExecuteMultiReader(commandText, 0, _DefaultCommandType, parametersBuilder, multiResultSetMap);
		}

		#endregion

		#region ExecuteNonQuery Overloads

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
					if (retry < _MaxRetryCount && OnConnectionLost(e))
						ReConnect(retry);
					else
						throw;
				}
			}

			return nAffectedRows;
		}

		public int ExecuteNonQuery(string commandText, Action<DbParameterBuilder> parametersBuilder = null)
		{
			return ExecuteNonQuery(commandText, 0, _DefaultCommandType, parametersBuilder);
		}

		#endregion

		#region CreateDataAdapter for backward compatibility with some old applications

		public DbDataAdapter CreateDataAdapter(string selectCommandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder, int oraResultSets = 1)
		{
			DbDataAdapter dbDataAdapter = _ProviderFactory.CreateDataAdapter();

			dbDataAdapter.SelectCommand = CreateCommand(selectCommandText, commandTimeout, commandType, parametersBuilder);

			OnReaderExecuting(dbDataAdapter.SelectCommand, oraResultSets);

			return dbDataAdapter;
		}

		public DbDataAdapter CreateDataAdapter(string selectCommandText, Action<DbParameterBuilder> parametersBuilder, int oraResultSets = 1)
		{
			return CreateDataAdapter(selectCommandText, 0, _DefaultCommandType, parametersBuilder, oraResultSets);
		}

		#endregion

		#region Load result sets into dynamic data (ExpandoObject List)

		protected string[] GetVisibleFieldNames(DbDataReader reader)
		{
			string[] visibleFieldNames = new string[reader.VisibleFieldCount];

			for (int i = 0; i < reader.VisibleFieldCount; i++)
				visibleFieldNames[i] = reader.GetName(i);

			return visibleFieldNames;
		}

		private dynamic CreateExpando(DbDataReader reader, string[] visibleFieldNames)
		{
			IDictionary<string, object> expandoObject = new ExpandoObject();

			if (visibleFieldNames == null)
				visibleFieldNames = GetVisibleFieldNames(reader);

			for (int i = 0; i < visibleFieldNames.Length; i++)
				expandoObject.Add(visibleFieldNames[i], reader[i]);

			return expandoObject;
		}

		private IEnumerable<dynamic> LoadDynamicData(DbDataReader reader)
		{
			string[] visibleFieldNames = GetVisibleFieldNames(reader);

			while (reader.Read())
				yield return CreateExpando(reader, visibleFieldNames);
		}

		public List<dynamic> ListDynamicResultSet(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder)
		{
			using (DbDataReader reader = CreateReader(commandText, commandTimeout, commandType, parametersBuilder))
			{
				return LoadDynamicData(reader).ToList();
			}
		}

		public List<dynamic> ListDynamicResultSet(string commandText, Action<DbParameterBuilder> parametersBuilder)
		{
			return ListDynamicResultSet(commandText, 0, _DefaultCommandType, parametersBuilder);
		}

		public List<List<dynamic>> ListDynamicResultSets(string commandText, int commandTimeout, CommandType commandType, Action<DbParameterBuilder> parametersBuilder, int oraResultSets = 1 /* For Oracle only */)
		{
			List<List<dynamic>> resultSets = new List<List<dynamic>>(oraResultSets);

			using (DbDataReader reader = CreateReader(commandText, commandTimeout, commandType, parametersBuilder, oraResultSets))
			{
				do
				{
					resultSets.Add(LoadDynamicData(reader).ToList());
				} while (reader.NextResult());
			}

			return resultSets;
		}

		public List<List<dynamic>> ListDynamicResultSets(string commandText, Action<DbParameterBuilder> parametersBuilder, int oraResultSets = 1 /* For Oracle only */)
		{
			return ListDynamicResultSets(commandText, 0, _DefaultCommandType, parametersBuilder, oraResultSets);
		}

		#endregion

		private void ReConnect(int retrying)
		{
			if (_Connection != null)
				if (_Connection.State != ConnectionState.Closed)
				{
					_Connection.Close();

					if (retrying > 0)
						Thread.Sleep(retrying * _IncreasingDelayRetry);	// retrying starts at 0, increases delay time for every retry.

					_Connection.Open();
				}
		}

		#region Transaction

		private DbTransactionManager _TransactionManager;

		#region Flat Transaction Methods
		public void BeginTransaction()
		{
			_TransactionManager.BeginTransaction();
		}

		public void BeginTransaction(IsolationLevel isolationLevel)
		{
			_TransactionManager.BeginTransaction(isolationLevel);
		}

		public void CommitTransaction()
		{
			_TransactionManager.Commit();
		}

		public void RollbackTransaction()
		{
			_TransactionManager.Rollback();
		}
		#endregion

		#region Auto-Scope Transaction Methods
		public DbTransactionScope NewTransactionScope()
		{
			return new DbTransactionScope(_TransactionManager);
		}

		public DbTransactionScope NewTransactionScope(IsolationLevel isolationLevel)
		{
			return new DbTransactionScope(_TransactionManager, isolationLevel);
		}
		#endregion

		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _Connection != null)
			{
				if (_Connection.State != ConnectionState.Closed)
				{
					_TransactionManager.Dispose();
					_Connection.Dispose();			// Close()
				}

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
