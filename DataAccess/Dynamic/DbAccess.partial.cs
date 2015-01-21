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

		protected DbParameter ExecuteStoredProcedure(StoredProcedureRequest request, Action<DbDataReader> readAction, out List<DbParameter> outputParameters)
		{
			if (readAction == null)
				throw new ArgumentNullException("resultsReader");
			if (request == null)
				throw new ArgumentNullException("request");
			if (string.IsNullOrWhiteSpace(request.CommandText))
				throw new ArgumentNullException("request.CommandText");

			List<DbParameter> outParameters = null;
			DbParameter returnParameter = null;

			using (DbDataReader reader = CreateReader(request.CommandText, request.CommandTimeout, request.CommandType, parameters =>
			{
				parameters.Derive(request.InputParameters);

				var dbParameters = parameters.Command.Parameters.OfType<DbParameter>();
				outParameters = dbParameters.Where(p => (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output) && !string.IsNullOrEmpty(p.ParameterName)).ToList();
				returnParameter = dbParameters.Where(p => p.Direction == ParameterDirection.ReturnValue).FirstOrDefault();
			}, 0))
			{
				readAction(reader);
			}

			outputParameters = outParameters;

			return returnParameter;
		}

		public StoredProcedureResponse ExecuteStoredProcedure(StoredProcedureRequest request)
		{
			StoredProcedureResponse spResponse = new StoredProcedureResponse();
			List<DbParameter> outputParameters;
			DbParameter returnParameter = ExecuteStoredProcedure(request, reader =>
				{
					bool isFirstResultSetVoid = false;

					do
					{
						spResponse.ResultSets.Add(LoadDynamicData(reader).ToList());

						if (spResponse.ResultSets.Count == 1 && reader.FieldCount == 0)
							isFirstResultSetVoid = true;
					} while (reader.NextResult());

					if (spResponse.ResultSets.Count == 1 && spResponse.ResultSets[0].Count == 0 && isFirstResultSetVoid)
						spResponse.ResultSets.Clear();
				}, out outputParameters);

			if (outputParameters != null)
			{
				IDictionary<string, object> expandoDictionary = spResponse.OutputParameters = new BindableDynamicObject();
				foreach (DbParameter op in outputParameters)
					expandoDictionary.Add(op.ParameterName.TrimParameterPrefix(), op.Value);
			}

			if (returnParameter != null)
				spResponse.ReturnValue = returnParameter.Value;

			return spResponse;
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
