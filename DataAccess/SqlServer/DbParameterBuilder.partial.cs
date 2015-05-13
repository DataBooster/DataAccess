using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.SqlServer.Server;

namespace DbParallel.DataAccess
{
	partial class DbParameterBuilder
	{
		/// <summary>
		/// Add a Table-Valued Parameter (SQL Server 2008+)
		/// </summary>
		/// <param name="parameterName">The name of the parameter</param>
		/// <param name="typeName">(Optional): The name of a compatible type created on the server</param>
		/// <returns>A SqlParameter object</returns>
		public SqlParameter AddTableValue(string parameterName, string typeName = "")
		{
			SqlCommand sqlCommand = _DbCommand as SqlCommand;
			SqlParameter parameter = sqlCommand.CreateParameter();

			parameter.ParameterName = parameterName;
			parameter.SqlDbType = SqlDbType.Structured;

			if (!string.IsNullOrEmpty(typeName))
				parameter.TypeName = typeName;

			sqlCommand.Parameters.Add(parameter);

			return parameter;
		}

		/// <summary>
		/// Add a Table-Valued Parameter (SQL Server 2008+) with a DataTable value
		/// </summary>
		/// <param name="parameterName">The name of the parameter</param>
		/// <param name="dataTable">An object derived from DataTable to stream rows of data to the table-valued parameter</param>
		/// <returns>A SqlParameter object</returns>
		public SqlParameter AddTableValue(string parameterName, DataTable dataTable)
		{
			SqlParameter sqlParameter = AddTableValue(parameterName);

			sqlParameter.Value = dataTable.AsParameterValue();

			return sqlParameter;
		}

		/// <summary>
		/// Add a Table-Valued Parameter (SQL Server 2008+) with a DbDataReader value
		/// </summary>
		/// <param name="parameterName">The name of the parameter</param>
		/// <param name="dataReader">An object derived from DbDataReader to stream rows of data to the table-valued parameter</param>
		/// <returns>A SqlParameter object</returns>
		public SqlParameter AddTableValue(string parameterName, DbDataReader dataReader)
		{
			SqlParameter sqlParameter = AddTableValue(parameterName);

			sqlParameter.Value = dataReader.AsParameterValue();

			return sqlParameter;
		}

		/// <summary>
		/// Add a Table-Valued Parameter (SQL Server 2008+) with a collection value
		/// </summary>
		/// <param name="parameterName">The name of the parameter</param>
		/// <param name="records">An object derived from IEnumerable&lt;SqlDataRecord&gt;, IEnumerable&lt;IDictionary&lt;string, object&gt;&gt; or IEnumerable&lt;object&gt; (a collection of anonymous or named type instances) </param>
		/// <returns>A SqlParameter object</returns>
		public SqlParameter AddTableValue<T>(string parameterName, IEnumerable<T> records)
		{
			SqlParameter sqlParameter = AddTableValue(parameterName);

			sqlParameter.Value = records.AsParameterValue();

			return sqlParameter;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2015 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2015-05-11
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
