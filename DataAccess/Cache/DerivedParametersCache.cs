using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace DbParallel.DataAccess
{
	public static partial class DerivedParametersCache
	{
		private class StoredProcedureDictionary : CacheDictionary<string, DbParameterCollection>
		{
			internal StoredProcedureDictionary()
				: base(StringComparer.OrdinalIgnoreCase)
			{
			}
		}

		private static ConcurrentDictionary<string, StoredProcedureDictionary> _CacheRoot;	// By ConnectionDataSource/ConnectionString
		private static TimeSpan _ExpireInterval;
		public static TimeSpan ExpireInterval
		{
			get { return _ExpireInterval; }
			set { _ExpireInterval = value; }
		}

		static DerivedParametersCache()
		{
			_CacheRoot = new ConcurrentDictionary<string, StoredProcedureDictionary>(StringComparer.OrdinalIgnoreCase);
			_ExpireInterval = TimeSpan.FromHours(1);	// Default 1 hour
		}

		#region Private Basic Operations
		private static DbParameterCollection GetCache(string connectionDataSource, string storedProcedure)
		{
			StoredProcedureDictionary spDictionary;
			DbParameterCollection paramColl;

			if (_CacheRoot.TryGetValue(connectionDataSource, out spDictionary))
				if (spDictionary.TryGetValue(storedProcedure, _ExpireInterval, out paramColl))
					return paramColl;

			return null;
		}

		private static void SetCache(string connectionDataSource, string storedProcedure, DbParameterCollection parameters)
		{
			StoredProcedureDictionary spDictionary;

			if (_CacheRoot.TryGetValue(connectionDataSource, out spDictionary))
				spDictionary.AddOrUpdate(storedProcedure, parameters);
			else
			{
				spDictionary = new StoredProcedureDictionary();
				spDictionary.TryAdd(storedProcedure, parameters);
				_CacheRoot.TryAdd(connectionDataSource, spDictionary);
			}
		}

		private static ICollection<string> ListCache(string connectionDataSource)
		{
			StoredProcedureDictionary spDictionary;

			if (_CacheRoot.TryGetValue(connectionDataSource, out spDictionary))
				return spDictionary.Keys;
			else
				return new List<string>();
		}

		private static int RemoveCache(string connectionDataSource, IEnumerable<string> storedProcedures)
		{
			StoredProcedureDictionary spDictionary;

			if (_CacheRoot.TryGetValue(connectionDataSource, out spDictionary))
				if (storedProcedures != null)
					return spDictionary.TryRemove(storedProcedures);

			return 0;
		}
		#endregion

		static private string GetConnectionDataSource(this DbConnection dbConnection)
		{
			if (dbConnection == null)
				throw new ArgumentNullException("dbConnection");

			string connectionDataSource = dbConnection.DataSource;

			if (string.IsNullOrEmpty(connectionDataSource))
				return dbConnection.ConnectionString;
			else
				return connectionDataSource;
		}

		static internal ICollection<string> ListStoredProcedures(DbConnection dbConnection)
		{
			return ListCache(dbConnection.GetConnectionDataSource());
		}

		static internal int RemoveStoredProcedures(DbConnection dbConnection, IEnumerable<string> storedProcedures)
		{
			return RemoveCache(dbConnection.GetConnectionDataSource(), storedProcedures);
		}

		static internal bool DeriveParameters(DbCommand dbCommand, IDictionary<string, IConvertible> explicitParameters, bool refresh)
		{
			if (dbCommand == null)
				throw new ArgumentNullException("dbCommand");

			string connectionDataSource = dbCommand.Connection.GetConnectionDataSource();
			string storedProcedure = dbCommand.CommandText;
			if (string.IsNullOrWhiteSpace(storedProcedure))
				throw new ArgumentNullException("dbCommand.CommandText");

			DbParameterCollection derivedParameters = null;

			if (dbCommand.CommandType == CommandType.StoredProcedure)
				if (refresh || (derivedParameters = GetCache(connectionDataSource, storedProcedure)) == null)
				{
					using (DbCommand cmd = dbCommand.Clone())
					{
						DbDeriveParameters(cmd);
						derivedParameters = cmd.Parameters;
					}

					SetCache(connectionDataSource, storedProcedure, derivedParameters);
				}

			if (derivedParameters == null)
			{
				TransferParameters(dbCommand, explicitParameters);
				return false;
			}
			else
			{
				TransferParameters(dbCommand, derivedParameters, explicitParameters);
				return true;
			}
		}

		static private void TransferParameters(DbCommand dbCommand, IDictionary<string, IConvertible> explicitParameters)
		{
			if (explicitParameters == null)
				return;

			Dictionary<string, DbParameter> specifiedParameters = dbCommand.Parameters.OfType<DbParameter>()
				.Where(p => !string.IsNullOrEmpty(p.ParameterName) && p.ParameterName.TrimParameterPrefix().Length > 0)
				.ToDictionary(p => p.ParameterName.TrimParameterPrefix(), StringComparer.OrdinalIgnoreCase);

			DbParameter dbParameter;
			string explicitName;

			foreach (var p in explicitParameters)
			{
				explicitName = p.Key.TrimParameterPrefix();

				if (explicitName.Length > 0)
					if (specifiedParameters.TryGetValue(explicitName, out dbParameter))
						dbParameter.Value = p.Value;
					else
					{
						dbParameter = dbCommand.CreateParameter();
						dbParameter.ParameterName = p.Key;
						dbParameter.Value = p.Value;
						dbCommand.Parameters.Add(dbParameter);
					}
			}
		}

		static private void TransferParameters(DbCommand dbCommand, DbParameterCollection derivedParameters, IDictionary<string, IConvertible> explicitParameters)
		{
			Dictionary<string, IConvertible> specifiedParameters = dbCommand.Parameters.OfType<DbParameter>()
				.Where(p => !string.IsNullOrEmpty(p.ParameterName) && p.ParameterName.TrimParameterPrefix().Length > 0)
				.ToDictionary(p => p.ParameterName.TrimParameterPrefix(), v => v.Value as IConvertible, StringComparer.OrdinalIgnoreCase);

			if (explicitParameters != null)
			{
				string explicitName;

				foreach (var p in explicitParameters)
				{
					explicitName = p.Key.TrimParameterPrefix();

					if (explicitName.Length > 0)
						specifiedParameters[explicitName] = p.Value;
				}
			}

			DbParameter dbParameter;
			IConvertible specifiedParameterValue;

			dbCommand.Parameters.Clear();

			for (int i = 0; i < derivedParameters.Count; i++)
			{
				dbParameter = derivedParameters[i].Clone();
				if (dbParameter == null)
				{
					dbParameter = dbCommand.CreateParameter();
					MemberwiseCopy(derivedParameters[i], dbParameter, null);
				}

				if (specifiedParameters.TryGetValue(dbParameter.ParameterName.TrimParameterPrefix(), out specifiedParameterValue))
				{
					if (dbParameter.IsUnpreciseDecimal())
						dbParameter.ResetDbType();		// To solve OracleTypeException: numeric precision specifier is out of range (1 to 38).

					dbParameter.Value = specifiedParameterValue;
				}

				dbCommand.Parameters.Add(dbParameter);
			}

			OmitUnspecifiedInputParameters(dbCommand);	// Remove unspecified optional parameters
		}

		static internal DbCommand Clone(this DbCommand sourceCommand)
		{
			ICloneable source = sourceCommand as ICloneable;

			if (source == null)
			{
				DbCommand cmd = sourceCommand.Connection.CreateCommand();

				MemberwiseCopy(sourceCommand, cmd, new HashSet<string>(new string[] { "Connection" }));

				return cmd;
			}
			else
				return source.Clone() as DbCommand;
		}

		static internal DbParameter Clone(this DbParameter sourceParameter)
		{
			ICloneable source = sourceParameter as ICloneable;

			return (source == null) ? null : source.Clone() as DbParameter;
		}

		static internal void MemberwiseCopy<T>(T source, T target, ICollection<string> excludeMembers = null)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (target == null)
				throw new ArgumentNullException("target");

			Type tp = source.GetType();
			var fields = tp.GetFields(BindingFlags.Public | BindingFlags.Instance);
			var properties = tp.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (FieldInfo fi in fields)
				if (!fi.IsInitOnly && (excludeMembers == null || !excludeMembers.Contains(fi.Name)))
				{
					try
					{
						fi.SetValue(target, fi.GetValue(source));
					}
					catch
					{
					}
				}

			foreach (PropertyInfo pi in properties)
				if (pi.CanRead && pi.CanWrite && (excludeMembers == null || !excludeMembers.Contains(pi.Name)))
				{
					try
					{
						pi.SetValue(target, pi.GetValue(source, null), null);
					}
					catch
					{
					}
				}
		}

		static internal string TrimParameterPrefix(this string ParameterName)
		{
			return ParameterName.TrimStart('@', ':');
		}

		static private bool IsUnpreciseDecimal(this DbParameter dbParameter)
		{
			return (dbParameter.DbType == DbType.Decimal && (dbParameter as IDbDataParameter).Precision == 0);
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
//	Created Date:		2014-12-29
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
