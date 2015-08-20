using System;
using System.Data.Common;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	public abstract class DbContextBase : IDisposable
	{
		private DbAccess _DbAccess;

		public DbContextBase(DbProviderFactory dbProviderFactory, string connectionString)
		{
			_DbAccess = new DbAccess(dbProviderFactory, connectionString);
		}

		public virtual StoredProcedureResponse ExecuteProcedure(string sp, IDictionary<string, object> parameters = null)
		{
			return _DbAccess.ExecuteStoredProcedure(new StoredProcedureRequest(sp, parameters));
		}

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _DbAccess != null)
			{
				_DbAccess.Dispose();
				_DbAccess = null;
			}
		}
		#endregion
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2015 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2015-08-19
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
