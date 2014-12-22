using System;
using System.Linq;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Dynamic;

namespace DbParallel.DataAccess
{
	public partial class DbAccess
	{
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
