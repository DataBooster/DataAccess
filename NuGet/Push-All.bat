@ECHO OFF
CD /D %~dp0

IF /i {%1} == {} GOTO :Usage
IF /i {%1} == {-h} GOTO :Usage
IF /i {%1} == {-help} GOTO :Usage

SET SQLPKG=nupkg\SqlServer\DataBooster.SqlServer.%1.nupkg
SET OMGPKG=nupkg\Oracle.Managed\DataBooster.Oracle.Managed.%1.nupkg
SET ODPPKG=nupkg\Oracle.ODP\DataBooster.Oracle.ODP.%1.nupkg
SET DDTPKG=nupkg\Oracle.DataDirect\DataBooster.Oracle.DataDirect.%1.nupkg
SET PREPARED=true

FOR %%P IN (%SQLPKG% %OMGPKG% %ODPPKG% %DDTPKG%) DO (
IF NOT EXIST %%P (
ECHO %%P does not exist!
SET PREPARED=false
)
)

IF {%PREPARED%} == {false} (
COLOR 0C
PAUSE
COLOR
GOTO :EOF
)

FOR %%P IN (%SQLPKG% %OMGPKG% %ODPPKG% %DDTPKG%) DO ..\.nuget\NuGet.exe Push %%P

GOTO :EOF

:Usage
ECHO.
ECHO Usage:
ECHO     Push-All.bat version
ECHO.
ECHO Example:
ECHO     Push-All.bat 1.7.8.8

GOTO :EOF