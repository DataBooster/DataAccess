@ECHO OFF
CD /D %~dp0
COPY SqlServer\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /y
..\.nuget\NuGet.exe pack ..\DataAccess\DataAccess.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg
