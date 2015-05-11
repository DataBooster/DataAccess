using System;
using System.Data;
using System.Collections.Generic;

namespace DbParallel.DataAccess
{
	internal static class ParameterConvert
	{
		public static DataTable ToDataTable<T>(this IEnumerable<T> dynObjects) where T : IDictionary<string, object>
		{
			if (dynObjects == null)
				return null;

			DataTable dataTable = PrepareDataColumns(dynObjects);

			dataTable.BeginLoadData();
			LoadDataRows(dataTable, dynObjects);
			dataTable.EndLoadData();

			return dataTable;
		}

		private static DataTable PrepareDataColumns<T>(IEnumerable<T> dynObjects) where T : IDictionary<string, object>
		{
			DataTable dataTable = new DataTable();
			DataColumnCollection tableColumns = dataTable.Columns;
			HashSet<string> pendingColumns = new HashSet<string>();

			foreach (var dynObj in dynObjects)
			{
				foreach (var p in dynObj)
				{
					if (pendingColumns.Contains(p.Key))
					{
						if (p.Value != null && !Convert.IsDBNull(p.Value))
						{
							tableColumns[p.Key].DataType = p.Value.GetType().GetNonNullableType();
							pendingColumns.Remove(p.Key);
						}
					}
					else if (!tableColumns.Contains(p.Key))
					{
						if (p.Value == null || Convert.IsDBNull(p.Value))
						{
							tableColumns.Add(p.Key);
							pendingColumns.Add(p.Key);
						}
						else
							tableColumns.Add(p.Key, p.Value.GetType().GetNonNullableType());
					}
				}

				if (pendingColumns.Count == 0)
					break;
			}

			return dataTable;
		}

		private static void LoadDataRows<T>(DataTable dataTable, IEnumerable<T> dynObjects) where T : IDictionary<string, object>
		{
			DataRow row;

			foreach (var dynObj in dynObjects)
			{
				row = dataTable.NewRow();

				foreach (var p in dynObj)
					row[p.Key] = (p.Value == null) ? DBNull.Value : p.Value;

				dataTable.Rows.Add(row);
			}
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
//	Created Date:		2015-05-08
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
