using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Reflection;
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
		private static readonly HashSet<string> _NonCopyProperties = new HashSet<string>(/*new string[] { "Value" }*/);
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

		static public bool DeriveParameters(DbCommand dbCommand, IDictionary<string, IConvertible> explicitParameters, bool refresh)
		{
			if (dbCommand == null)
				throw new ArgumentNullException("dbCommand");
			if (dbCommand.Connection == null)
				throw new ArgumentNullException("dbCommand.Connection");

			string connectionString = dbCommand.Connection.ConnectionString;
			string storedProcedure = dbCommand.CommandText;

			if (string.IsNullOrWhiteSpace(storedProcedure))
				throw new ArgumentNullException("dbCommand.CommandText");

			DbParameterCollection derivedParameters = null;

			if (dbCommand.CommandType == CommandType.StoredProcedure)
				if (refresh || (derivedParameters = GetCache(connectionString, storedProcedure)) == null)
				{
					derivedParameters = RefreshParameters(dbCommand);
					SetCache(connectionString, storedProcedure, derivedParameters);
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
				.Where(p => !string.IsNullOrEmpty(p.ParameterName) && p.ParameterName.TrimPrefix().Length > 0)
				.ToDictionary(p => p.ParameterName.TrimPrefix(), StringComparer.OrdinalIgnoreCase);

			DbParameter dbParameter;
			string explicitName;

			foreach (var p in explicitParameters)
			{
				explicitName = p.Key.TrimPrefix();

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
				.Where(p => !string.IsNullOrEmpty(p.ParameterName) && p.ParameterName.TrimPrefix().Length > 0)
				.ToDictionary(p => p.ParameterName.TrimPrefix(), v => v.Value as IConvertible, StringComparer.OrdinalIgnoreCase);

			if (explicitParameters != null)
			{
				string explicitName;

				foreach (var p in explicitParameters)
				{
					explicitName = p.Key.TrimPrefix();

					if (explicitName.Length > 0)
						specifiedParameters[p.Key] = p.Value;
				}
			}

			DbParameter dbParameter;
			IConvertible specifiedParameterValue;
			int specifiedPosition = 0;

			dbCommand.Parameters.Clear();

			for (int i = 0; i < derivedParameters.Count; i++)
			{
				dbParameter = derivedParameters[i].Clone();
				if (dbParameter == null)
				{
					dbParameter = dbCommand.CreateParameter();
					MemberwiseCopy(derivedParameters[i], dbParameter, _NonCopyProperties);
				}

				if (specifiedParameters.TryGetValue(dbParameter.ParameterName.TrimPrefix(), out specifiedParameterValue))
				{
					if (dbParameter.IsUnpreciseDecimal())	// To solve OracleTypeException: numeric precision specifier is out of range (1 to 38).
						dbParameter.ResetDbType();

					dbParameter.Value = specifiedParameterValue;

					specifiedPosition = i;
				}

				dbCommand.Parameters.Add(dbParameter);
			}

			// Remove all trailing unspecified optional parameters
			for (int i = dbCommand.Parameters.Count - 1; i > specifiedPosition; i--)
				if (dbCommand.Parameters[i].Direction == ParameterDirection.Input)
					dbCommand.Parameters.RemoveAt(i);
				else
					break;
		}

		static private DbParameter Clone(this DbParameter source)
		{
			ICloneable sourceParameter = source as ICloneable;

			return (sourceParameter == null) ? null : sourceParameter.Clone() as DbParameter;
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

		static private string TrimPrefix(this string ParameterName)
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
//	(Keep clean code rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
