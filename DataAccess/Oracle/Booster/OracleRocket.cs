#if ORACLE
using System;
using System.Data;
using System.Collections.Generic;
using DDTek.Oracle;

namespace DbParallel.DataAccess.Oracle.Booster
{
	class OracleRocket : IDisposable
	{
		private readonly OracleCommand _Command;
		private readonly int _BulkSize;

		#region Filling Control
		private readonly int[] _AssociativeArrayParameterIds;
		private Array[] _AssociativeArrayValues;
		private bool _AssociativeArrayInitialized;
		private int _FillingCount;
		#endregion

		public OracleRocket(OracleCommand command, int[] associativeArrayParameterIds, int bulkSize)
		{
			_Command = command;
			_BulkSize = bulkSize;

			_AssociativeArrayParameterIds = associativeArrayParameterIds;
			_AssociativeArrayValues = new Array[associativeArrayParameterIds.Length];
			_AssociativeArrayInitialized = false;
			_FillingCount = 0;
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

		public bool AddRow(params object[] values)
		{
			System.Diagnostics.Debug.Assert(values.Length == _AssociativeArrayParameterIds.Length, "The number of input parameters does not match with Associative Array Parameters!");

			if (_FillingCount == 0)
				InitializeAssociativeArray(values);

			for (int i = 0; i < _AssociativeArrayParameterIds.Length; i++)
				_AssociativeArrayValues[i].SetValue(values[i], _FillingCount);

			return (++_FillingCount == _BulkSize);
		}

		private void InitializeAssociativeArray(object[] values)
		{
			if (_AssociativeArrayInitialized == false)
			{
				OracleParameterCollection parameters = _Command.Parameters;

				for (int i = 0; i < _AssociativeArrayParameterIds.Length; i++)
					parameters[_AssociativeArrayParameterIds[i]].Value = _AssociativeArrayValues[i] = Array.CreateInstance(values[i].GetType(), _BulkSize);

				_AssociativeArrayInitialized = true;
			}
		}

		public int Launch()
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

		public void Dispose()
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
