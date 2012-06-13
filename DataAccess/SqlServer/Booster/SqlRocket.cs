using System;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;

namespace DbParallel.DataAccess.Booster.SqlServer
{
	class SqlRocket : DbRocket
	{
		private readonly SqlBulkCopy _BulkCopy;
		private readonly SqlConnection _Connection;

		private DataTable _BufferTable;
		private bool _BufferTableInitialized;

		public SqlRocket(SqlBulkCopy bulkCopy, SqlConnection dbConnection, int bulkSize)
			: base(bulkSize)
		{
			_BulkCopy = bulkCopy;
			_Connection = dbConnection;

			_BufferTable = new DataTable();
			_BufferTableInitialized = false;
		}

		public override bool AddRow(params IConvertible[] values)
		{
			if (_FillingCount == 0)
				InitializeBufferTable(values.Length);
			else
				Debug.Assert(values.Length == _BufferTable.Columns.Count, "The number of input parameters does not match with initial columns!");

			for (int i = 0; i < values.Length; i++)
				if (values[i] == null)
					values[i] = DBNull.Value;

			_BufferTable.Rows.Add(values);

			return (++_FillingCount == _BulkSize);
		}

		private void InitializeBufferTable(int columns)
		{
			if (_BufferTableInitialized == false)
			{
				if (_BulkCopy.ColumnMappings.Count > 0)
					Debug.Assert(columns == _BulkCopy.ColumnMappings.Count, "The number of input parameters does not match with column mappings!");

				for (int i = 0; i < columns; i++)
					_BufferTable.Columns.Add(null, typeof(IConvertible));

				_BufferTableInitialized = true;
			}
		}

		public override int Launch()
		{
			int fillingCount = _FillingCount;

			if (_FillingCount > 0)
			{
				if (_Connection.State == ConnectionState.Closed)
					_Connection.Open();

				_BulkCopy.WriteToServer(_BufferTable);

				_BufferTable.Clear();
				_FillingCount = 0;
			}

			return fillingCount;
		}

		public override void Dispose()
		{
			Launch();

			_BulkCopy.Close();

			if (_Connection.State == ConnectionState.Open)
				_Connection.Close();
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
//	Created Date:		2012-06-09
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
