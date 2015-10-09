@ECHO OFF
CD /D %~dp0

IF NOT EXIST nupkg\StoredProcedureData MKDIR nupkg\StoredProcedureData

..\.nuget\NuGet.exe pack ..\DataAccess\Dynamic\StoredProcedureData\StoredProcedureData.csproj -IncludeReferencedProjects -Symbols -Properties Configuration=Release;Platform=AnyCPU -OutputDirectory nupkg\StoredProcedureData
