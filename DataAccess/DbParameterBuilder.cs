using System;
using System.Data;
using System.Data.Common;

namespace DbParallel.DataAccess
{
	public partial class DbParameterBuilder
	{
		private readonly DbCommand _DbCommand;
		public DbCommand Command { get { return _DbCommand; } }

		public DbParameterBuilder(DbCommand dbCommand)
		{
			_DbCommand = dbCommand;
		}

		public DbParameter this[string parameterName]
		{
			get { return _DbCommand.Parameters[parameterName]; }
			set { _DbCommand.Parameters[parameterName] = value; }
		}

		public DbParameter this[int index]
		{
			get { return _DbCommand.Parameters[index]; }
			set { _DbCommand.Parameters[index] = value; }
		}

		public DbParameter AddReturn(string parameterName = "RETURN_VALUE", DbType dbType = DbType.Int32, int nSize = 0)
		{
			DbParameter parameter = _DbCommand.CreateParameter();
			parameter.ParameterName = parameterName;
			parameter.DbType = dbType;
			parameter.Direction = ParameterDirection.ReturnValue;

			if (nSize > 0)
				parameter.Size = nSize;

			_DbCommand.Parameters.Add(parameter);

			return parameter;
		}

		public DbParameter Add()
		{
			DbParameter parameter = _DbCommand.CreateParameter();
			_DbCommand.Parameters.Add(parameter);

			return parameter;
		}

		public DbParameter Add(string parameterName, object oValue, int nSize = 0)
		{
			DbParameter parameter = _DbCommand.CreateParameter();
			parameter.ParameterName = parameterName;
			parameter.Value = (oValue == null) ? DBNull.Value : oValue;

			if (nSize > 0)
				parameter.Size = nSize;

			_DbCommand.Parameters.Add(parameter);

			return parameter;
		}

		public DbParameter AddOutput(string parameterName, int nSize = 0)
		{
			DbParameter parameter = _DbCommand.CreateParameter();
			parameter.ParameterName = parameterName;
			parameter.Direction = ParameterDirection.Output;

			if (nSize > 0)
				parameter.Size = nSize;

			_DbCommand.Parameters.Add(parameter);

			return parameter;
		}

		public DbParameter AddOutput(string parameterName, DbType dbType, int nSize = 0)
		{
			DbParameter parameter = _DbCommand.CreateParameter();
			parameter.ParameterName = parameterName;
			parameter.DbType = dbType;
			parameter.Direction = ParameterDirection.Output;

			if (nSize > 0)
				parameter.Size = nSize;

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
