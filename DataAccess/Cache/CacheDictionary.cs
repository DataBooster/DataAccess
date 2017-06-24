using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace DbParallel.DataAccess
{
	public class CacheDictionary<TKey, TValue> : ConcurrentDictionary<TKey, CacheItem<TValue>>
	{
		public CacheDictionary()
		{
		}

		public CacheDictionary(IEqualityComparer<TKey> comparer)
			: base(comparer)
		{
		}

		public CacheDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection)
			: base(collection.Select(pair => new KeyValuePair<TKey, CacheItem<TValue>>(pair.Key, new CacheItem<TValue>(pair.Value))))
		{
		}

		public CacheDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
			: base(collection.Select(pair => new KeyValuePair<TKey, CacheItem<TValue>>(pair.Key, new CacheItem<TValue>(pair.Value))), comparer)
		{
		}

		public TValue AddOrUpdate(TKey key, TValue value)
		{
			CacheItem<TValue> existingItem;

			if (value == null)
				return (TryRemove(key, out existingItem)) ? existingItem.Value : value;

			if (TryGetValue(key, out existingItem))
				existingItem.Value = value;
			else
				TryAdd(key, value);

			return value;
		}

		public bool TryAdd(TKey key, TValue value)
		{
			return TryAdd(key, new CacheItem<TValue>(value));
		}

		public int TryRemove(IEnumerable<TKey> keys)
		{
			CacheItem<TValue> existingItem;
			int cntRemoved = 0;

			foreach (TKey key in keys)
				if (TryRemove(key, out existingItem))
					cntRemoved++;

			return cntRemoved;
		}

		public bool TryRemove(TKey key)
		{
			CacheItem<TValue> existingItem;

			return TryRemove(key, out existingItem);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			CacheItem<TValue> existingItem;

			if (TryGetValue(key, out existingItem))
			{
				value = existingItem.Value;
				return true;
			}
			else
			{
				value = default(TValue);
				return false;
			}
		}

		public bool TryGetValue(TKey key, TimeSpan validityPeriod, out TValue value)
		{
			CacheItem<TValue> existingItem;

			if (TryGetValue(key, out existingItem))
				if (validityPeriod <= TimeSpan.Zero || DateTime.Now - existingItem.LastRefreshed < validityPeriod)
				{
					value = existingItem.Value;
					return true;
				}

			value = default(TValue);
			return false;
		}

		public TValue GetOrAdd(TKey key, TValue value)
		{
			CacheItem<TValue> existingItem;

			if (TryGetValue(key, out existingItem))
				return existingItem.Value;
			else
			{
				if (value != null)
					TryAdd(key, value);

				return value;
			}
		}

		public int RemoveExpiredKeys(TimeSpan validityPeriod)
		{
			return TryRemove(GetExpiredKeys(validityPeriod).ToList());
		}

		public IEnumerable<TKey> GetExpiredKeys(TimeSpan validityPeriod)
		{
			if (validityPeriod <= TimeSpan.Zero)
				yield break;

			DateTime expiryDate = DateTime.Now - validityPeriod;

			foreach (var kvp in this)
				if (kvp.Value.LastRefreshed <= expiryDate)
					yield return kvp.Key;
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
//	Updated Host:		https://github.com/DataBooster/DataAccess
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep code clean rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
