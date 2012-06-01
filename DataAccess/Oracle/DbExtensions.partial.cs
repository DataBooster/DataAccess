#if ORACLE
using DDTek.Oracle;

namespace DbParallel.DataAccess
{
	public static partial class DbExtensions
	{
		public static OracleParameter SetOracleType(this OracleParameter oracleParameter, OracleDbType oraType)
		{
			oracleParameter.OracleDbType = oraType;
			return oracleParameter;
		}

		public static OracleParameter SetOracleScale(this OracleParameter oracleParameter, byte nScale)
		{
			oracleParameter.Scale = nScale;
			return oracleParameter;
		}

		public static OracleParameter SetOraclePrecision(this OracleParameter oracleParameter, byte nPrecision)
		{
			oracleParameter.Precision = nPrecision;
			return oracleParameter;
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
