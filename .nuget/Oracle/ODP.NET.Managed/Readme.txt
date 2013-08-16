Installation Notes:

A dependency NuGet package "odp.net.managed" - Oracle Data Provider for .NET (ODP.NET) Managed Driver
http://www.nuget.org/packages/odp.net.managed is also installed automatically.


Three setting items have been added into your config file:

<configuration>
	<connectionStrings>
		<add name="$rootnamespace$.MainConnection" providerName="Oracle.ManagedDataAccess.Client" connectionString="Data Source=SAMPLEDB;User Id=/"/>
		<add name="$rootnamespace$.AuxConnection" providerName="System.Data.SqlClient" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=SAMPLEDB;Integrated Security=True" />
	</connectionStrings>
	<appSettings>
		<add key="$rootnamespace$.MainPackage" value="SCHEMA.PACKAGE." />
	</appSettings>
</configuration>

Please update them as your practical environment.

And three *.sample.cs files have been copied into your project\DataAccess folder for quick starts.


Open Source:
http://databooster.codeplex.com