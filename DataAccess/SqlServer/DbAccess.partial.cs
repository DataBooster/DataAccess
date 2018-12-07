using System;
using System.Data.SqlClient;

namespace DbParallel.DataAccess
{
	partial class DbAccess
	{
		partial void OnSqlContextLost(Exception dbException, ref RetryAction retryAction, ref bool processed)
		{
			if (processed)
				return;

			if (_Connection is SqlConnection)
			{
				SqlException e = dbException as SqlException;

				if (e == null)
					retryAction = RetryAction.None;
				else
					switch (e.Number)	// sys.messages
					{
						case 233:
						case -2:
						case 10054:
							retryAction = RetryAction.Reconnect;
							break;
						case 201:		// Procedure or function '%.*ls' expects parameter '%.*ls', which was not supplied.
						case 206:		// Operand type clash: %ls is incompatible with %ls.
						case 257:		// Implicit conversion from data type %ls to %ls is not allowed. Use the CONVERT function to run this query.
						case 8144:		// Procedure or function %.*ls has too many arguments specified.
							retryAction = RetryAction.RefreshParameters;
							break;
						// To add other cases
						default:
							retryAction = RetryAction.None;
							break;
					}

				processed = true;
			}
		}

		partial void OnSqlReconnecting(ref bool processed)
		{
			if (processed)
				return;

			SqlConnection conn = _Connection as SqlConnection;

			if (conn == null)
				return;

			SqlConnection.ClearPool(conn);

			processed = true;
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2012 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2013-07-‎29
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
