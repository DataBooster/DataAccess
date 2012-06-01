#if ORACLE
using System;
using DDTek.Oracle;

namespace DbParallel.DataAccess
{
	public partial class DbAccess
	{
		partial void OnOracleConnectionLoss(Exception dbException, ref bool canRetry)
		{
			if (_Connection is OracleConnection)
			{
				OracleException e = dbException as OracleException;

				if (e == null)
					canRetry = false;
				else
					switch (e.Number)
					{
						case 4068: canRetry = true; break;
						// To add other cases
						default: canRetry = false; break;
					}
			}
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
