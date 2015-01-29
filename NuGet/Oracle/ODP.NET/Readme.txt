Installation Notes:

This edition of library requires "Oracle Data Provider for .NET (ODP.NET)"
http://www.oracle.com/technetwork/developer-tools/visual-studio/overview/index.html
or http://www.oracle.com/technetwork/topics/dotnet/index-085163.html on your own license.
Please note that you must install it manually (in GAC) before installing this edition.


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