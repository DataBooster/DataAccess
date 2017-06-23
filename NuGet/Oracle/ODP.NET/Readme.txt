http://www.nuget.org/packages/DataBooster.Oracle.ODP/
Installation Notes:

This edition of library requires "Oracle Data Provider for .NET, Unmanaged Driver (ODP.NET 4 or later)"
http://www.oracle.com/technetwork/topics/dotnet/downloads/index.html.
You should install it manually (in GAC) before installing this package.


Three setting items have been added into your config file:

<configuration>
	<connectionStrings>
		<add name="$rootnamespace$.MainConnection" providerName="Oracle.DataAccess.Client" connectionString="Data Source=SAMPLEDB;User Id=/"/>
		<add name="$rootnamespace$.AuxConnection" providerName="System.Data.SqlClient" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=SAMPLEDB;Integrated Security=SSPI" />
	</connectionStrings>
	<appSettings>
		<add key="$rootnamespace$.MainPackage" value="SCHEMA.PACKAGE." />
	</appSettings>
</configuration>

Please update them as your practical environment.

And three *.sample.cs files have been copied into your project\DataAccess folder for quick starts.


Open Source:
http://databooster.codeplex.com
https://github.com/DataBooster/DataAccess