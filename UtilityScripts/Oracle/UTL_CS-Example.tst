PL/SQL Developer Test script 3.0
9
DECLARE
	RC1	SYS_REFCURSOR;
BEGIN
	-- Call the target procedure, return the RC1
	XYZ.TPW_TEST.TEST_RESULT_SET(SYSDATE, RC1);

	-- Generate C# class BasePosition from RC1
	:csCode	:= XYZ.UTL_CS.GEN_CS_CLASS(RC1, 'BasePosition', 1, 0);
END;
1
:csCode
1
<CLOB>
112
2
tGen_Code
tCode
