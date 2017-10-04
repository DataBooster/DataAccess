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
					switch (e.Number)
					{
						case 233:
						case -2:
							retryAction = RetryAction.Reconnect;
							break;
						case 201:
						case 8144:
							retryAction = RetryAction.RefreshParameters;
							break;
						// To add other cases
						default: retryAction = RetryAction.None;
							break;
					}

				processed = true;
			}
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
