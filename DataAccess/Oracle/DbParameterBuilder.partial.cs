#if ORACLE
using DDTek.Oracle;

namespace DbParallel.DataAccess
{
	public partial class DbParameterBuilder
	{
		public OracleParameter AddOracleAssociativeArray(string parameterName, OracleDbType oraType)
		{
			OracleCommand oraCommand = _DbCommand as OracleCommand;
			OracleParameter parameter = oraCommand.CreateParameter();

			parameter.ParameterName = parameterName;
			parameter.OracleDbType = oraType;
			parameter.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
			oraCommand.Parameters.Add(parameter);

			return parameter;
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
