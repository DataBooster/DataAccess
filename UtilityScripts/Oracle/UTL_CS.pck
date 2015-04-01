CREATE OR REPLACE PACKAGE XYZ.UTL_CS IS

----------------------------------------------------------------------------------------------------
--
--	Copyright 2012 Abel Cheng
--	This source code is subject to terms and conditions of the Apache License, Version 2.0.
--	See http://www.apache.org/licenses/LICENSE-2.0.
--	All other rights reserved.
--	You must not remove this notice, or any other, from this software.
--
--	Original Author:	Abel Cheng <abelcys@gmail.com>
--	Created Date:		2012-06-06
--	Primary Host:		http://databooster.codeplex.com
--	Change Log:
--	Author				Date			Comment
--
--
--
--
--	(Keep code clean rather than complicated code plus long comments.)
--
----------------------------------------------------------------------------------------------------

FUNCTION GEN_CS_CLASS
(
	RC				IN OUT SYS_REFCURSOR,
	inClass			VARCHAR2,			-- The C# class name to be generated.
	inNullable		PLS_INTEGER	:= 1,	-- To generate Nullable<T> class member: 1 = Yes; 0 = No.
	inReportChange	PLS_INTEGER	:= 0	-- To generate change trackers (ReportPropertyChanging and ReportPropertyChanged): 1 = Yes; 0 = No.
) RETURN CLOB;

END UTL_CS;
/
CREATE OR REPLACE PACKAGE BODY XYZ.UTL_CS IS

g_Property_Fmt	CONSTANT VARCHAR2(512)	:= '
		[DataMember]
		public %s %s
		{
			get
			{
				return _%s;
			}
			set
			{
				if (_%s != value)
				{
					On%sChanging(value);
					%s
					_%s = value;
					%s
					On%sChanged();
				}
			}
		}
		private %s _%s;
		partial void On%sChanging(%s value);
		partial void On%sChanged();
';

g_Class_Fmt_Begin	CONSTANT VARCHAR2(256)	:= '
	[Serializable()]
	[DataContractAttribute(IsReference=true)]
	public partial class %s
	{
		#region Primitive Properties
';

g_Class_Fmt_End		CONSTANT VARCHAR2(128)	:= '
		#endregion
	}
';


FUNCTION CONVERT_COL_NAME_TO_CS
(
	inName	VARCHAR2
)	RETURN	VARCHAR2 IS
	tUpper_Name	VARCHAR2(32)	:= UPPER(inName);
BEGIN
	IF inName = tUpper_Name THEN
		RETURN INITCAP(inName);
	ELSE
		RETURN inName;
	END IF;
END CONVERT_COL_NAME_TO_CS;

FUNCTION CONVERT_COL_TYPE_TO_CS
(
	inType		PLS_INTEGER,
	inPrecision	PLS_INTEGER,
	inScale		PLS_INTEGER,
	inNullable	BOOLEAN
)	RETURN		VARCHAR2 IS
BEGIN
	IF inType IN (1, 9, 96, 286, 287, 69, 104, 112, 288) THEN
		RETURN 'String';
	ELSIF inType = 2 THEN
		IF inScale = 0 THEN
			IF inPrecision = 1 THEN
				IF inNullable THEN
					RETURN 'Boolean?';
				ELSE
					RETURN 'Boolean';
				END IF;
			ELSIF inPrecision = 2 THEN
				IF inNullable THEN
					RETURN 'SByte?';
				ELSE
					RETURN 'SByte';
				END IF;
			ELSIF inPrecision <= 4 THEN
				IF inNullable THEN
					RETURN 'Int16?';
				ELSE
					RETURN 'Int16';
				END IF;
			ELSIF inPrecision <= 9 THEN
				IF inNullable THEN
					RETURN 'Int32?';
				ELSE
					RETURN 'Int32';
				END IF;
			ELSIF inPrecision <= 18 THEN
				IF inNullable THEN
					RETURN 'Int64?';
				ELSE
					RETURN 'Int64';
				END IF;
			ELSE
				IF inNullable THEN
					RETURN 'Decimal?';
				ELSE
					RETURN 'Decimal';
				END IF;
			END IF;
		ELSE
			IF inNullable THEN
				RETURN 'Decimal?';
			ELSE
				RETURN 'Decimal';
			END IF;
		END IF;
	ELSIF inType = 100 THEN
		IF inNullable THEN
			RETURN 'Float?';
		ELSE
			RETURN 'Float';
		END IF;
	ELSIF inType = 101 THEN
		IF inNullable THEN
			RETURN 'Double?';
		ELSE
			RETURN 'Double';
		END IF;
	ELSIF inType IN (12, 187, 188, 232) THEN
		IF inNullable THEN
			RETURN 'DateTime?';
		ELSE
			RETURN 'DateTime';
		END IF;
	ELSIF inType IN (23, 24, 95, 113, 114) THEN
		RETURN 'Byte[]';
	ELSE
		RETURN NULL;
	END IF;
END CONVERT_COL_TYPE_TO_CS;

FUNCTION GEN_CS_CLASS_MEMBER
(
	inName			VARCHAR2,
	inType			PLS_INTEGER,
	inPrecision		PLS_INTEGER,
	inScale			PLS_INTEGER,
	inNullable		BOOLEAN,
	inReportChange	BOOLEAN
)	RETURN VARCHAR2 IS
	tCs_Name	VARCHAR2(30)	:= CONVERT_COL_NAME_TO_CS(inName);
	tCs_Type	VARCHAR2(32)	:= CONVERT_COL_TYPE_TO_CS(inType, inPrecision, inScale, inNullable);
	tReportChanging	VARCHAR2(64):= '';
	tReportChanged	VARCHAR2(64):= '';
BEGIN
	IF inReportChange THEN
		tReportChanging	:= UTL_LMS.FORMAT_MESSAGE('ReportPropertyChanging("%s");', tCs_Name);
		tReportChanged	:= UTL_LMS.FORMAT_MESSAGE('ReportPropertyChanged("%s");', tCs_Name);
	END IF;

	RETURN UTL_LMS.FORMAT_MESSAGE(g_Property_Fmt, tCs_Type, tCs_Name, tCs_Name, tCs_Name, tCs_Name, tReportChanging, tCs_Name, tReportChanged, tCs_Name, tCs_Type, tCs_Name, tCs_Name, tCs_Type, tCs_Name);
END GEN_CS_CLASS_MEMBER;


FUNCTION GEN_CS_CLASS
(
	RC				IN OUT SYS_REFCURSOR,
	inClass			VARCHAR2,			-- The C# class name to be generated.
	inNullable		PLS_INTEGER	:= 1,	-- To generate Nullable<T> class member: 1 = Yes; 0 = No.
	inReportChange	PLS_INTEGER	:= 0	-- To generate change trackers (ReportPropertyChanging and ReportPropertyChanged): 1 = Yes; 0 = No.
)	RETURN CLOB IS
	tCursor			INTEGER	:= DBMS_SQL.TO_CURSOR_NUMBER(RC);
	tCol_Cnt		INTEGER;
	tDesc_Tbl		DBMS_SQL.DESC_TAB;
	tGen_Code		CLOB	:= UTL_LMS.FORMAT_MESSAGE(g_Class_Fmt_Begin, inClass);
	tNullable		BOOLEAN	:= SYS.DIUTIL.INT_TO_BOOL(inNullable);
	tReportChange	BOOLEAN	:= SYS.DIUTIL.INT_TO_BOOL(inReportChange);
BEGIN
	DBMS_SQL.DESCRIBE_COLUMNS(tCursor, tCol_Cnt, tDesc_Tbl);

	FOR i IN 1..tDesc_Tbl.COUNT
	LOOP
		DBMS_LOB.APPEND(tGen_Code, GEN_CS_CLASS_MEMBER(tDesc_Tbl(i).COL_NAME, tDesc_Tbl(i).COL_TYPE, tDesc_Tbl(i).COL_PRECISION, tDesc_Tbl(i).COL_SCALE, tNullable, tReportChange));
	END LOOP;

	DBMS_SQL.CLOSE_CURSOR(tCursor);

	DBMS_LOB.APPEND(tGen_Code, g_Class_Fmt_End);

	RETURN tGen_Code;
END GEN_CS_CLASS;


END UTL_CS;
/
