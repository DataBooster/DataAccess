using System;
using System.Data;
using System.Data.Common;

namespace DbParallel.DataAccess
{
	static partial class DerivedParametersCache
	{
		static partial void OracleDeriveParameters(DbCommand dbCmd, ref bool processed);
		static partial void SqlDeriveParameters(DbCommand dbCmd, ref bool processed);
		static private void DbDeriveParameters(DbCommand dbCmd)
		{
			bool hasBeenProcessed = false;

			OracleDeriveParameters(dbCmd, ref hasBeenProcessed);
			SqlDeriveParameters(dbCmd, ref hasBeenProcessed);
		}

		static partial void OracleOmitUnspecifiedInputParameters(DbCommand dbCmd, ref bool processed);
		static partial void SqlOmitUnspecifiedInputParameters(DbCommand dbCmd, ref bool processed);
		static private void OmitUnspecifiedInputParameters(DbCommand dbCmd)
		{
			bool hasBeenProcessed = false;

			OracleOmitUnspecifiedInputParameters(dbCmd, ref hasBeenProcessed);
			SqlOmitUnspecifiedInputParameters(dbCmd, ref hasBeenProcessed);
		}

		static partial void OracleAdaptParameterValue(DbParameter dbParameter, string specifiedParameterValue, ref bool processed);
		static partial void SqlAdaptParameterValue(DbParameter dbParameter, string specifiedParameterValue, ref bool processed);
		static private void AdaptParameterValue(DbParameter dbParameter, object specifiedParameterValue)
		{
			switch (dbParameter.DbType)
			{
				case DbType.Object:
				case DbType.Binary:
					{
						string strValue = specifiedParameterValue as string;
						if (strValue != null)
						{
							bool hasBeenProcessed = false;

							OracleAdaptParameterValue(dbParameter, strValue, ref hasBeenProcessed);
							SqlAdaptParameterValue(dbParameter, strValue, ref hasBeenProcessed);

							if (hasBeenProcessed)
								return;
						}
					}
					break;

				case DbType.String:
				case DbType.StringFixedLength:
				case DbType.AnsiString:
				case DbType.AnsiStringFixedLength:
					{
						byte[] uploadedBinary = TryCastAsBytes(specifiedParameterValue);
						if (uploadedBinary != null)
						{
							dbParameter.Value = uploadedBinary.DecodeBytesToString();
							return;
						}
					}
					break;

				case DbType.Xml:
					break;

				default:
					{
						string strValue = specifiedParameterValue as string;
						if (strValue != null && string.IsNullOrWhiteSpace(strValue))
						{
							dbParameter.Value = DBNull.Value;
							return;
						}
					}
					break;
			}

			//if (dbParameter.IsUnpreciseDecimal())
			//    dbParameter.ResetDbType();		// To solve OracleTypeException: numeric precision specifier is out of range (1 to 38).

			dbParameter.Value = specifiedParameterValue;
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
//	Created Date:		2014-12-23
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
