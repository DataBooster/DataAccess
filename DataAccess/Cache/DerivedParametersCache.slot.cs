using System.Data.Common;

namespace DbParallel.DataAccess
{
	static partial class DerivedParametersCache
	{
		static partial void OracleDeriveParameters(DbCommand dbCmd, ref bool processed);
		static partial void SqlDeriveParameters(DbCommand dbCmd, ref bool processed);
		static private void DbDeriveParameters(DbCommand dbCmd)
		{
			bool hasBeenProcessed = false;

			OracleDeriveParameters(dbCmd, ref hasBeenProcessed);
			SqlDeriveParameters(dbCmd, ref hasBeenProcessed);
		}

		static partial void OracleOmitUnspecifiedInputParameters(DbCommand dbCmd, ref bool processed);
		static partial void SqlOmitUnspecifiedInputParameters(DbCommand dbCmd, ref bool processed);
		static private void OmitUnspecifiedInputParameters(DbCommand dbCmd)
		{
			bool hasBeenProcessed = false;

			OracleOmitUnspecifiedInputParameters(dbCmd, ref hasBeenProcessed);
			SqlOmitUnspecifiedInputParameters(dbCmd, ref hasBeenProcessed);
		}

		static partial void OracleAdaptParameterValueStringToBinary(DbParameter dbParameter, string specifiedParameterValue, ref bool processed);
		static partial void SqlAdaptParameterValueStringToBinary(DbParameter dbParameter, string specifiedParameterValue, ref bool processed);
		static private bool AdaptParameterValueStringToBinary(DbParameter dbParameter, string specifiedParameterValue)
		{
			bool hasBeenProcessed = false;

			OracleAdaptParameterValueStringToBinary(dbParameter, specifiedParameterValue, ref hasBeenProcessed);
			SqlAdaptParameterValueStringToBinary(dbParameter, specifiedParameterValue, ref hasBeenProcessed);

			return hasBeenProcessed;
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
//	Created Date:		2014-12-23
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
