#if ORACLE
using System;
using System.Data;
using System.Collections.Generic;
using System.Diagnostics;
#if DATADIRECT
using DDTek.Oracle;
#else // ODP.NET
using Oracle.DataAccess.Client;
#endif

namespace DbParallel.DataAccess.Booster.Oracle
{
	class OracleRocket : DbRocket
	{
		private readonly OracleCommand _Command;

		private readonly int[] _AssociativeArrayParameterIds;
		private Array[] _AssociativeArrayValues;

		public OracleRocket(OracleCommand command, int[] associativeArrayParameterIds, int bulkSize)
			: base(bulkSize)
		{
			OracleParameterCollection parameters = command.Parameters;
			_Command = command;

			_AssociativeArrayParameterIds = associativeArrayParameterIds;
			_AssociativeArrayValues = new Array[associativeArrayParameterIds.Length];

			for (int i = 0; i < _AssociativeArrayParameterIds.Length; i++)
				parameters[_AssociativeArrayParameterIds[i]].Value = _AssociativeArrayValues[i] = new IConvertible[_BulkSize];
		}

		public OracleRocket(OracleCommand command, int bulkSize)
			: this(command, SearchAssociativeArrayParameters(command.Parameters), bulkSize)
		{
		}

		static internal int[] SearchAssociativeArrayParameters(OracleParameterCollection parameters)
		{
			List<int> associativeArrayParameterList = new List<int>();

			for (int p = 0; p < parameters.Count; p++)
				if (parameters[p].CollectionType == OracleCollectionType.PLSQLAssociativeArray)
					associativeArrayParameterList.Add(p);

			return associativeArrayParameterList.ToArray();
		}

		public override bool AddRow(params IConvertible[] values)
		{
			Debug.Assert(values.Length == _AssociativeArrayParameterIds.Length, "The number of input parameters does not match with Associative Array Parameters!");

			for (int i = 0; i < _AssociativeArrayParameterIds.Length; i++)
				_AssociativeArrayValues[i].SetValue(values[i] ?? DBNull.Value, _FillingCount);

			return (++_FillingCount == _BulkSize);
		}

		public override int Launch()
		{
			int fillingCount = _FillingCount;

			if (_FillingCount > 0)
			{
				OracleParameterCollection parameters = _Command.Parameters;

				if (_FillingCount < _BulkSize)
					for (int i = 0; i < _AssociativeArrayParameterIds.Length; i++)
						parameters[_AssociativeArrayParameterIds[i]].Value = ShrinkArray(_AssociativeArrayValues[i], _FillingCount);

				if (_Command.Connection.State == ConnectionState.Closed)
					_Command.Connection.Open();

				_Command.ExecuteNonQuery();

				if (_FillingCount < _BulkSize)
					for (int i = 0; i < _AssociativeArrayParameterIds.Length; i++)
						parameters[_AssociativeArrayParameterIds[i]].Value = _AssociativeArrayValues[i];

				_FillingCount = 0;
			}

			return fillingCount;
		}

		private static Array ShrinkArray(Array oldArray, int newSize)
		{
			Array newArray = Array.CreateInstance(oldArray.GetType().GetElementType(), newSize);

			Array.Copy(oldArray, newArray, newSize);

			return newArray;
		}

		public override void Dispose()
		{
			Launch();

			if (_Command.Connection.State == ConnectionState.Open)
				_Command.Connection.Close();
		}
	}
}
#endif


////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2012 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2012-05-18
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
