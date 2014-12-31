#if ORACLE
using System.Data;
using System.Data.Common;

#if DATADIRECT
using DDTek.Oracle;
#elif ODP_NET	// ODP.NET
using Oracle.DataAccess.Client;
#else			// ODP.NET.Managed
using Oracle.ManagedDataAccess.Client;
#endif

namespace DbParallel.DataAccess
{
	internal static partial class DerivedParametersCache
	{
		static partial void OracleDeriveParameters(DbCommand dbCmd)
		{
			OracleCommand oraCmd = dbCmd as OracleCommand;

			if (oraCmd != null)
				OracleCommandBuilder.DeriveParameters(oraCmd);
		}

		static partial void OracleOmitUnspecifiedInputParameters(DbCommand dbCmd)
		{
			OracleCommand oraCmd = dbCmd as OracleCommand;

			if (oraCmd != null)
			{
				if (oraCmd.CommandType == CommandType.StoredProcedure && oraCmd.BindByName == false)
					oraCmd.BindByName = true;

				OracleParameter oraParameter;

				for (int i = oraCmd.Parameters.Count - 1; i >= 0; i--)
				{
					oraParameter = oraCmd.Parameters[i];

					if (oraParameter.Value == null && oraParameter.Direction == ParameterDirection.Input)
						oraCmd.Parameters.RemoveAt(i);
				}
			}
		}
	}
}

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Copyright 2014 Abel Cheng
//	This source code is subject to terms and conditions of the Apache License, Version 2.0.
//	See http://www.apache.org/licenses/LICENSE-2.0.
//	All other rights reserved.
//	You must not remove this notice, or any other, from this software.
//
//	Original Author:	Abel Cheng <abelcys@gmail.com>
//	Created Date:		2014-12-23
//	Original Host:		http://dbParallel.codeplex.com
//	Primary Host:		http://DataBooster.codeplex.com
//	Change Log:
//	Author				Date			Comment
//
//
//
//
//	(Keep clean code rather than complicated code plus long comments.)
//
////////////////////////////////////////////////////////////////////////////////////////////////////
