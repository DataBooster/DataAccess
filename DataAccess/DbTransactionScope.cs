using System;
using System.Data;

namespace DbParallel.DataAccess
{
	public class DbTransactionScope : IDisposable
	{
		private readonly DbTransactionManager _TransactionManager;
		private bool _IsCompleted;

		internal DbTransactionScope(DbTransactionManager dbTransactionManager, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
		{
			_TransactionManager = dbTransactionManager;
			_TransactionManager.EnterTransactionScope(isolationLevel);
		}

		public void Complete()
		{
			_IsCompleted = true;
		}

		#region IDisposable Members
		public void Dispose()
		{
			_TransactionManager.ExitTransactionScope(_IsCompleted);
		}
		#endregion
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
//	Created Date:		2013-07-31
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
