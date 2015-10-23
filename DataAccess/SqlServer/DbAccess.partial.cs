using System;
using System.Data.SqlClient;

namespace DbParallel.DataAccess
{
	partial class DbAccess
	{
		partial void OnSqlConnectionLost(Exception dbException, ref bool canRetry, ref bool processed)
		{
			if (processed)
				return;

			if (_Connection is SqlConnection)
			{
				SqlException e = dbException as SqlException;

				if (e == null)
					canRetry = false;
				else
					switch (e.Number)
					{
						case 233:
						case -2: canRetry = true; break;
						// To add other cases
						default: canRetry = false; break;
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
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
