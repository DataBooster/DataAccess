using System;

namespace DbParallel.DataAccess.Booster
{
	public abstract class DbRocket : IDisposable
	{
		protected readonly int _BulkSize;
		protected int _FillingCount;

		public DbRocket(int bulkSize)
		{
			_BulkSize = bulkSize;
			_FillingCount = 0;
		}

		public abstract bool AddRow(params IConvertible[] values);
		public abstract int Launch();

		public abstract void Dispose();
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
