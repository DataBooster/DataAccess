using System;
using System.Data.Common;

namespace DbParallel.DataAccess
{
	public partial class DbParameterBuilder
	{
		private readonly DbCommand _DbCommand;

		public DbParameterBuilder(DbCommand dbCommand)
		{
			_DbCommand = dbCommand;
		}

		public DbParameter Add()
		{
			DbParameter parameter = _DbCommand.CreateParameter();
			_DbCommand.Parameters.Add(parameter);

			return parameter;
		}

		public DbParameter Add(string parameterName, object oValue)
		{
			DbParameter parameter = _DbCommand.CreateParameter();
			parameter.ParameterName = parameterName;
			parameter.Value = (oValue == null) ? DBNull.Value : oValue;
			_DbCommand.Parameters.Add(parameter);

			return parameter;
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
