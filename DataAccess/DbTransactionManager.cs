using System;
using System.Data;
using System.Data.Common;
using System.Threading;

namespace DbParallel.DataAccess
{
	internal class DbTransactionManager : IDisposable
	{
		private readonly DbConnection _DbConnection;

		private DbTransaction _DbTransaction;
		internal DbTransaction Transaction
		{
			get { return _DbTransaction; }
		}

		#region Constructor
		public DbTransactionManager(DbConnection dbConnection)
		{
			_DbConnection = dbConnection;
			_DbTransaction = null;
		}
		#endregion

		private bool InFlatTransaction
		{
			get { return (_DbTransaction != null && _TransactionScopeLevel == 0); }
		}

		private bool InScopeTransaction
		{
			get { return (_DbTransaction != null && _TransactionScopeLevel > 0); }
		}

		#region Flat Transaction Mode

		public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
		{
			if (_TransactionScopeLevel == 0)
			{
				if (_DbTransaction != null)
					_DbTransaction.Commit();

				_DbTransaction = (isolationLevel == IsolationLevel.Unspecified) ? _DbConnection.BeginTransaction() : _DbConnection.BeginTransaction(isolationLevel);
			}
			else
				throw new InvalidOperationException("BeginTransaction (Flat Transaction) can not be embedded inside DbTransactionScope (Auto-Scope Transaction)");
		}

		public void Commit()
		{
			if (_TransactionScopeLevel == 0)
			{
				if (_DbTransaction != null)
				{
					_DbTransaction.Commit();
					_DbTransaction = null;
				}
			}
			else
				throw new InvalidOperationException("Commit (Flat Transaction) can not be embedded inside DbTransactionScope (Auto-Scope Transaction)");
		}

		public void Rollback()
		{
			if (_TransactionScopeLevel == 0)
			{
				if (_DbTransaction != null)
				{
					_DbTransaction.Rollback();
					_DbTransaction = null;
				}
			}
			else
				throw new InvalidOperationException("Rollback (Flat Transaction) can not be embedded inside DbTransactionScope (Auto-Scope Transaction)");
		}

		#endregion

		#region Auto-Scope Transaction Mode

		private int _TransactionScopeLevel;
		private bool _ContainRollbackScope;

		internal void EnterTransactionScope(IsolationLevel isolationLevel)
		{
			if (InFlatTransaction)
				throw new InvalidOperationException("DbTransactionScope (Auto-Scope Transaction) can not be embedded during a Flat Transaction");
			else
			{
				if (InScopeTransaction == false)
				{
					BeginTransaction(isolationLevel);
					_ContainRollbackScope = false;
				}

				Interlocked.Increment(ref _TransactionScopeLevel);
			}
		}

		internal void ExitTransactionScope(bool isCompleted)
		{
			if (InScopeTransaction)
			{
				if (isCompleted == false)
					_ContainRollbackScope = true;

				if (Interlocked.Decrement(ref _TransactionScopeLevel) == 0)
				{
					if (_ContainRollbackScope)
					{
						Rollback();
						_ContainRollbackScope = false;
					}
					else
						Commit();
				}
			}
			else
				throw new InvalidOperationException("DbTransactionScope must exit from a Auto-Scope Transaction");
		}

		#endregion

		#region IDisposable Members
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && _DbTransaction != null)
			{
				_DbTransaction.Dispose();
				_DbTransaction = null;
			}
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
