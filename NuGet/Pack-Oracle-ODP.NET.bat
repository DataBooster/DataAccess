copy Oracle\ODP.NET\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /Y
nuget pack ..\DataAccess\DataAccess.csproj -Prop Configuration=Release
