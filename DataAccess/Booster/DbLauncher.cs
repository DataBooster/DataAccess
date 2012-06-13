using System;
using System.Collections.Concurrent;

namespace DbParallel.DataAccess.Booster
{
	public abstract class DbLauncher : IDisposable
	{
		protected const int _MinMultipleRockets = 3;
		protected const int _MinBulkSize = 1000;
		protected const int _DefaultMultipleRockets = 6;
		protected const int _DefaultBulkSize = 500000;
		protected const int _CommandTimeout = 3600;

		protected readonly BlockingCollection<DbRocket> _FreeQueue;
		protected DbRocket _FillingRocket;

		protected readonly object _FillingLock;
		protected readonly ParallelExecuteWaitHandle _ExecutingHandle;

		private bool _Disposed = false;

		public DbLauncher()
		{
			_FreeQueue = new BlockingCollection<DbRocket>();

			_FillingLock = new object();
			_ExecutingHandle = new ParallelExecuteWaitHandle();
		}

		public void Post(params IConvertible[] values)
		{
			lock (_FillingLock)
			{
				if (_FillingRocket.AddRow(values))
				{
					_ExecutingHandle.StartNewTask(LaunchRocket, _FillingRocket);
					_FillingRocket = _FreeQueue.Take();
				}
			}
		}

		private void LaunchRocket(DbRocket rocket)
		{
			rocket.Launch();
			_FreeQueue.Add(rocket);
		}

		public void Complete()
		{
			lock (_FillingLock)
			{
				_FillingRocket.Launch();
				_ExecutingHandle.Wait();
			}
		}

		public void Dispose()
		{
			if (_Disposed == false)
			{
				Complete();

				foreach (DbRocket rocket in _FreeQueue)
					rocket.Dispose();

				if (_FillingRocket != null)
					_FillingRocket.Dispose();

				_ExecutingHandle.Dispose();

				_Disposed = true;
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
//	Created Date:		2012-06-10
//	Primary Host:		http://databooster.codeplex.com
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep clean code rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
