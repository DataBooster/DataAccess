﻿#if ORACLE
#if DATADIRECT
using DDTek.Oracle;
#elif ODP_NET	// ODP.NET
using Oracle.DataAccess.Client;
#else			// ODP.NET.Managed
using Oracle.ManagedDataAccess.Client;
#endif

namespace DbParallel.DataAccess
{
	partial class DbParameterBuilder
	{
		public OracleParameter AddAssociativeArray(string parameterName, OracleDbType oraType)
		{
			OracleCommand oraCommand = _DbCommand as OracleCommand;
			OracleParameter parameter = oraCommand.CreateParameter();

			parameter.ParameterName = parameterName;
			parameter.OracleDbType = oraType;
			parameter.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
			oraCommand.Parameters.Add(parameter);

			return parameter;
		}

		public OracleParameter AddRefCursor(string parameterName)
		{
			OracleParameter oraParam = AddOutput(parameterName) as OracleParameter;
			oraParam.OracleDbType = OracleDbType.RefCursor;

			return oraParam;
		}
	}
}
#endif

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
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
