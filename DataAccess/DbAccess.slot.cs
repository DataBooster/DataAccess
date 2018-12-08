using System;
using System.Threading;
using System.Data.Common;

namespace DbParallel.DataAccess
{
	partial class DbAccess
	{
		partial void OnOracleContextLost(Exception dbException, ref RetryAction retryAction, ref bool processed);
		partial void OnSqlContextLost(Exception dbException, ref RetryAction retryAction, ref bool processed);
		private RetryAction OnContextLost(Exception dbException)
		{
			RetryAction retryAction = RetryAction.None;
			bool hasBeenProcessed = false;

			if (_TransactionManager.Transaction == null)
			{
				OnOracleContextLost(dbException, ref retryAction, ref hasBeenProcessed);
				OnSqlContextLost(dbException, ref retryAction, ref hasBeenProcessed);
			}

			return retryAction;
		}

		partial void OnOracleReaderExecuting(DbCommand dbCmd, int resultSetCnt/* = 1 */, ref bool processed);
		private void OnReaderExecuting(DbCommand dbCmd, int resultSetCnt/* = 1*/)
		{
			bool hasBeenProcessed = false;

			OnOracleReaderExecuting(dbCmd, resultSetCnt, ref hasBeenProcessed);
		}

		partial void OnOracleReconnecting(ref bool processed);
		partial void OnSqlReconnecting(ref bool processed);
		private void OnReconnecting()
		{
			bool hasBeenProcessed = false;

			try
			{
				OnOracleReconnecting(ref hasBeenProcessed);
				OnSqlReconnecting(ref hasBeenProcessed);
			}
			catch
			{
				Thread.Sleep(618);
			}
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
//	Updated Host:		https://github.com/DataBooster/DataAccess
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
