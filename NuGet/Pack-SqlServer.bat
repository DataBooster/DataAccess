copy SqlServer\DataAccess.nuspec ..\DataAccess\DataAccess.nuspec /y
nuget pack ..\DataAccess\DataAccess.csproj -Prop Configuration=Release
