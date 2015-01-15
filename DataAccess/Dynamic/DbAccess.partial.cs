using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Dynamic;

namespace DbParallel.DataAccess
{
	partial class DbAccess
	{
		#region Load result sets into dynamic data

		protected string[] GetVisibleFieldNames(DbDataReader reader)
		{
			string[] visibleFieldNames = new string[reader.VisibleFieldCount];

			for (int i = 0; i < reader.VisibleFieldCount; i++)
				visibleFieldNames[i] = reader.GetName(i);

			return visibleFieldNames;
		}

		private ExpandoObject CreateExpando(DbDataReader reader, string[] visibleFieldNames)
		{
			ExpandoObject expandoObject = new ExpandoObject();
			IDictionary<string, object> expandoDictionary = expandoObject;

			if (visibleFieldNames == null)
				visibleFieldNames = GetVisibleFieldNames(reader);

			for (int i = 0; i < visibleFieldNames.Length; i++)
				expandoDictionary.Add(visibleFieldNames[i], reader[i]);

			return expandoObject;
		}

		private IEnumerable<BindableDynamicObject> LoadDynamicData(DbDataReader reader)
		{
			string[] visibleFieldNames = GetVisibleFieldNames(reader);

			while (reader.Read())
				yield return new BindableDynamicObject(CreateExpando(reader, visibleFieldNames));
		}

		public StoredProcedureResponse ExecuteStoredProcedure(StoredProcedureRequest request)
		{
			StoredProcedureResponse result = new StoredProcedureResponse();
			List<DbParameter> outputParameters = null;
			DbParameter returnParameter = null;

			if (string.IsNullOrWhiteSpace(request.CommandText))
				throw new ArgumentNullException("request.CommandText");

			using (DbDataReader reader = CreateReader(request.CommandText, request.CommandTimeout, request.CommandType, parameters =>
				{
					parameters.Derive(request.InputParameters);

					var dbParameters = parameters.Command.Parameters.OfType<DbParameter>();
					outputParameters = dbParameters.Where(p => (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output) && !string.IsNullOrEmpty(p.ParameterName)).ToList();
					returnParameter = dbParameters.Where(p => p.Direction == ParameterDirection.ReturnValue).FirstOrDefault();
				}, 0))
			{
				bool isFirstResultSetVoid = false;

				do
				{
					result.ResultSets.Add(LoadDynamicData(reader).ToList());

					if (result.ResultSets.Count == 1 && reader.FieldCount == 0)
						isFirstResultSetVoid = true;
				} while (reader.NextResult());

				if (result.ResultSets.Count == 1 && result.ResultSets[0].Count == 0 && isFirstResultSetVoid)
					result.ResultSets.Clear();
			}

			if (outputParameters != null)
			{
				IDictionary<string, object> expandoDictionary = result.OutputParameters = new BindableDynamicObject();
				foreach (DbParameter op in outputParameters)
					expandoDictionary.Add(op.ParameterName.TrimParameterPrefix(), op.Value);
			}

			if (returnParameter != null)
				result.ReturnValue = returnParameter.Value;

			return result;
		}

		public bool RefreshStoredProcedureParameters(string storedProcedureName)
		{
			if (string.IsNullOrWhiteSpace(storedProcedureName))
				throw new ArgumentNullException("storedProcedureName");

			using (DbCommand dbCmd = CreateCommand(storedProcedureName, 0, CommandType.StoredProcedure, null))
			{
				return DerivedParametersCache.DeriveParameters(dbCmd, null, true);
			}
		}

		public int RefreshStoredProcedureParameters(IEnumerable<string> storedProcedureNames)
		{
			int cnt = 0;

			foreach (string spName in storedProcedureNames)
				if (RefreshStoredProcedureParameters(spName))
					cnt++;

			return cnt;
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
//	Created Date:		2014-12-19
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
