using System;
using System.Threading;
using System.Threading.Tasks;

namespace DbParallel.DataAccess
{
	public class ParallelExecuteWaitHandle : IDisposable
	{
		protected ManualResetEventSlim _CompleteEvent;
		protected int _ExecutingCount;

		public ParallelExecuteWaitHandle(bool initialState = true)
		{
			_CompleteEvent = new ManualResetEventSlim(initialState);
			_ExecutingCount = 0;
		}

		protected virtual void EnterTask()
		{
			if (Interlocked.Increment(ref _ExecutingCount) == 1)
				_CompleteEvent.Reset();
		}

		protected virtual void ExitTask()
		{
			if (Interlocked.Decrement(ref _ExecutingCount) == 0)
				_CompleteEvent.Set();
		}

		public Task StartNewTask(Action action)
		{
			EnterTask();

			return Task.Factory.StartNew(() =>
			{
				try
				{
					action();
				}
				finally
				{
					ExitTask();
				}
			});
		}

		public Task StartNewTask<T>(Action<T> action, T state)
		{
			EnterTask();

			return Task.Factory.StartNew(context =>
			{
				try
				{
					action((T)context);
				}
				finally
				{
					ExitTask();
				}
			}, state);
		}

		public void Wait()
		{
			if (_ExecutingCount > 0)
				_CompleteEvent.Wait();
		}

		public void Wait(int millisecondsTimeout)
		{
			if (_ExecutingCount > 0)
				_CompleteEvent.Wait(millisecondsTimeout);
		}

		public void Wait(TimeSpan timeout)
		{
			if (_ExecutingCount > 0)
				_CompleteEvent.Wait(timeout);
		}

		public void Dispose()
		{
			if (_CompleteEvent != null)
			{
				_CompleteEvent.Dispose();
				_CompleteEvent = null;
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
//	Created Date:		2012-05-30
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
