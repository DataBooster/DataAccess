using System;
using System.Threading;

namespace DbParallel.DataAccess
{
	public class CacheItem<TValue> : IEquatable<CacheItem<TValue>>
	{
		private long _LastRefreshedBinary;
		public DateTime LastRefreshed
		{
			get { return DateTime.FromBinary(Interlocked.Read(ref _LastRefreshedBinary)); }
			private set { Interlocked.Exchange(ref _LastRefreshedBinary, value.ToBinary()); }
		}

		private TValue _Value;
		public TValue Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
				LastRefreshed = DateTime.Now;
			}
		}

		public CacheItem(TValue value)
		{
			Value = value;
		}

		public CacheItem()
		{
		}

		public bool Equals(CacheItem<TValue> other)
		{
			return _Value.Equals(other.Value);
		}
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
//	Created Date:		2015-03-31
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
