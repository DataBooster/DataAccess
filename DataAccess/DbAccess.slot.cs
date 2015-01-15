using System;
using System.Data.Common;

namespace DbParallel.DataAccess
{
	partial class DbAccess
	{
		partial void OnOracleConnectionLost(Exception dbException, ref bool canRetry);
		partial void OnSqlConnectionLost(Exception dbException, ref bool canRetry);
		private bool OnConnectionLost(Exception dbException)
		{
			bool canRetry = false;

			if (_TransactionManager.Transaction == null)
			{
				OnOracleConnectionLost(dbException, ref canRetry);
				OnSqlConnectionLost(dbException, ref canRetry);
			}

			return canRetry;
		}

		partial void OnOracleReaderExecuting(DbCommand dbCmd, int resultSetCnt/* = 1 */);
		private void OnReaderExecuting(DbCommand dbCmd, int resultSetCnt/* = 1*/)
		{
			OnOracleReaderExecuting(dbCmd, resultSetCnt);
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
//	Created Date:		2014-12-18
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
