#if ORACLE
using System;
using System.Collections.Generic;
#if DATADIRECT
using DDTek.Oracle;
#elif ODP_NET	// ODP.NET
using Oracle.DataAccess.Client;
#else			// ODP.NET.Managed
using Oracle.ManagedDataAccess.Client;
#endif

namespace DbParallel.DataAccess
{
	partial class DbParameterBuilder
	{
		/// <summary>
		/// Add an Associative Array Parameter (Oracle)
		/// </summary>
		/// <param name="parameterName">The name of the parameter</param>
		/// <param name="oraType">The OracleDbType of the parameter</param>
		/// <returns>An OracleParameter object</returns>
		public OracleParameter AddAssociativeArray(string parameterName, OracleDbType oraType)
		{
			OracleCommand oraCommand = _DbCommand as OracleCommand;
			OracleParameter parameter = oraCommand.CreateParameter();

			parameter.ParameterName = parameterName;
			parameter.OracleDbType = oraType;
			parameter.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
			oraCommand.Parameters.Add(parameter);

			return parameter;
		}

		/// <summary>
		/// Add an Associative Array Parameter (Oracle) with an Array value
		/// </summary>
		/// <typeparam name="T">A type implements IConvertible</typeparam>
		/// <param name="parameterName">The name of the parameter</param>
		/// <param name="oraType">The OracleDbType of the parameter</param>
		/// <param name="associativeArray">An Array value</param>
		/// <returns>An OracleParameter object</returns>
		public OracleParameter AddAssociativeArray<T>(string parameterName, OracleDbType oraType, T[] associativeArray) where T : IConvertible
		{
			OracleParameter oracleParameter = AddAssociativeArray(parameterName, oraType);

			oracleParameter.Value = associativeArray.AsParameterValue();

			return oracleParameter;
		}

		/// <summary>
		/// Add an Associative Array Parameter (Oracle) with an Array value
		/// </summary>
		/// <typeparam name="T">A type implements IConvertible</typeparam>
		/// <param name="parameterName">The name of the parameter</param>
		/// <param name="oraType">The OracleDbType of the parameter</param>
		/// <param name="associativeArray">A collection of simple values (implements IConvertible)</param>
		/// <returns>An OracleParameter object</returns>
		public OracleParameter AddAssociativeArray<T>(string parameterName, OracleDbType oraType, IEnumerable<T> associativeArray) where T : IConvertible
		{
			OracleParameter oracleParameter = AddAssociativeArray(parameterName, oraType);

			oracleParameter.Value = associativeArray.AsParameterValue();

			return oracleParameter;
		}

		/// <summary>
		/// <para>Add a PL/SQL REF CURSOR (SYS_REFCURSOR) Parameter (Oracle).</para>
		/// <para>Notes: The ParameterDirection defaults to Output.</para>
		/// </summary>
		/// <param name="parameterName">The name of the parameter</param>
		/// <returns>An OracleParameter object</returns>
		public OracleParameter AddRefCursor(string parameterName)
		{
			OracleParameter oraParam = AddOutput(parameterName) as OracleParameter;
			oraParam.OracleDbType = OracleDbType.RefCursor;

			return oraParam;
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
//	Created Date:		2012-03-23
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
