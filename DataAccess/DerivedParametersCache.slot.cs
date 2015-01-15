using System.Data.Common;

namespace DbParallel.DataAccess
{
	static partial class DerivedParametersCache
	{
		static partial void OracleDeriveParameters(DbCommand dbCmd);
		static partial void SqlDeriveParameters(DbCommand dbCmd);
		static private void DbDeriveParameters(DbCommand dbCmd)
		{
			OracleDeriveParameters(dbCmd);
			SqlDeriveParameters(dbCmd);
		}

		static partial void OracleOmitUnspecifiedInputParameters(DbCommand dbCmd);
		static partial void SqlOmitUnspecifiedInputParameters(DbCommand dbCmd);
		static private void OmitUnspecifiedInputParameters(DbCommand dbCmd)
		{
			OracleOmitUnspecifiedInputParameters(dbCmd);
			SqlOmitUnspecifiedInputParameters(dbCmd);
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
//	(Keep clean code rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
