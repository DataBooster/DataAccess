@ECHO OFF
CD /D %~dp0

IF /i {%1} == {} GOTO :Usage
IF /i {%1} == {-h} GOTO :Usage
IF /i {%1} == {-help} GOTO :Usage

SET PKGSQL=nupkg\SqlServer\DataBooster.SqlServer.%1.nupkg
SET PKGOMG=nupkg\Oracle.Managed\DataBooster.Oracle.Managed.%1.nupkg
SET PKGODP=nupkg\Oracle.ODP\DataBooster.Oracle.ODP.%1.nupkg
SET PKGDDT=nupkg\Oracle.DataDirect\DataBooster.Oracle.DataDirect.%1.nupkg
SET PKGPREPARED=true

FOR %%P IN (%PKGSQL% %PKGOMG% %PKGODP% %PKGDDT%) DO (
IF NOT EXIST %%P (
ECHO %%P does not exist!
SET PKGPREPARED=false
)
)

IF {%PKGPREPARED%} == {false} (
COLOR 0C
PAUSE
COLOR
GOTO :EOF
)

FOR %%P IN (%PKGSQL% %PKGOMG% %PKGODP% %PKGDDT%) DO ..\.nuget\NuGet.exe Push %%P

GOTO :EOF

:Usage
ECHO.
ECHO Usage:
ECHO     Push-All.bat version
ECHO.
ECHO Example:
ECHO     Push-All.bat 1.7.8.8
