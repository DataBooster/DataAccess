using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	internal static partial class DerivedParametersCache
	{
		class StoredProcedureDictionary : Dictionary<string, DbParameterCollection>
		{
			public StoredProcedureDictionary()
				: base(StringComparer.OrdinalIgnoreCase)
			{
			}
		}

		private static readonly object _DerivedParametersCacheLock = new object();
		private static Dictionary<string, StoredProcedureDictionary> _CacheRoot;	// By ConnectionString

		static DerivedParametersCache()
		{
			_CacheRoot = new Dictionary<string, StoredProcedureDictionary>(StringComparer.OrdinalIgnoreCase);
		}

		private static DbParameterCollection GetCache(string connectionString, string storedProcedure)
		{
			StoredProcedureDictionary spDictionary;
			DbParameterCollection dbParameters = null;

			lock (_DerivedParametersCacheLock)
			{
				if (_CacheRoot.TryGetValue(connectionString, out spDictionary))
					spDictionary.TryGetValue(storedProcedure, out dbParameters);
			}

			return dbParameters;
		}

		private static void SetCache(string connectionString, string storedProcedure, DbParameterCollection parameters)
		{
			StoredProcedureDictionary spDictionary;

			lock (_DerivedParametersCacheLock)
			{
				if (_CacheRoot.TryGetValue(connectionString, out spDictionary) == false)
				{
					spDictionary = new StoredProcedureDictionary();
					_CacheRoot.Add(connectionString, spDictionary);
				}

				spDictionary[storedProcedure] = parameters;
			}
		}

		static private DbParameterCollection RefreshParameters(DbCommand spCmd)
		{
			using (DbCommand cmd = spCmd.Connection.CreateCommand())
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = spCmd.CommandText;
				cmd.Transaction = spCmd.Transaction;

				DbDeriveParameters(cmd);

				return cmd.Parameters;
			}
		}

		static public bool DeriveParameters(DbCommand dbCommand, bool refresh)
		{
			if (dbCommand == null)
				throw new ArgumentNullException("dbCommand");
			if (dbCommand.Connection == null)
				throw new ArgumentNullException("dbCommand.Connection");
			if (dbCommand.CommandType != CommandType.StoredProcedure)
				throw new ArgumentOutOfRangeException("dbCommand.CommandType", "Only supports CommandType.StoredProcedure!");

			string connectionString = dbCommand.Connection.ConnectionString;
			string storedProcedure = dbCommand.CommandText;

			if (string.IsNullOrWhiteSpace(storedProcedure))
				throw new ArgumentNullException("dbCommand.CommandText");

			DbParameterCollection derivedParameters = null;

			if (refresh || (derivedParameters = GetCache(connectionString, storedProcedure)) == null)
			{
				derivedParameters = RefreshParameters(dbCommand);
				SetCache(connectionString, storedProcedure, derivedParameters);
			}

			// TODO

			return (derivedParameters != null);
		}
	}
}
