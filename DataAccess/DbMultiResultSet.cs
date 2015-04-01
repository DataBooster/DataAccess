using System;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	public class DbMultiResultSet
	{
		internal class DbResultAdapter<T> where T : class, new()
		{
			private readonly ICollection<T> _ResultSet;
			public ICollection<T> ResultSet
			{
				get { return _ResultSet; }
			}

			private readonly DbFieldMap<T> _FieldMap;
			public DbFieldMap<T> FieldMap
			{
				get { return _FieldMap; }
			}

			public DbResultAdapter(ICollection<T> resultSet, Action<DbFieldMap<T>> resultMap)
			{
				_ResultSet = resultSet;
				_FieldMap = new DbFieldMap<T>();
				_FieldMap.PrepareResultMap(resultMap);
			}
		}

		private readonly ArrayList _MultiResultSet;
		public int Count { get { return _MultiResultSet.Count; } }

		public DbMultiResultSet()
		{
			_MultiResultSet = new ArrayList();
		}

		public void Add<T>(ICollection<T> resultSet, Action<DbFieldMap<T>> resultMap = null) where T : class, new()
		{
			_MultiResultSet.Add(new DbResultAdapter<T>(resultSet, resultMap));
		}

		public void Add<T>(ref ICollection<T> resultSet, Action<DbFieldMap<T>> resultMap = null) where T : class, new()
		{
			_MultiResultSet.Add(new DbResultAdapter<T>(resultSet, resultMap));
		}

		internal void ReadAll(DbDataReader reader)
		{
			for (int rs = 0; rs < _MultiResultSet.Count; rs++)
			{
				dynamic resultAdapter = _MultiResultSet[rs];

				if (resultAdapter.ResultSet == null)
					continue;

				while (reader.Read())
					resultAdapter.ResultSet.Add(resultAdapter.FieldMap.ReadNew(reader));

				if (reader.NextResult() == false)
					break;
			}
		}
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2014 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2014-07-17
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
