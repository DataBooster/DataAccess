copy Oracle\DataDirect\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /Y
nuget pack ..\DataAccess\DataAccess.csproj -Prop Configuration=Release
