http://www.nuget.org/packages/DataBooster.Oracle.DataDirect/
Installation Notes:

This edition of library requires "Progress DataDirect Connect ADO.NET PROVIDER FOR ORACLE"
(https://www.progress.com/net/data-sources/oracle) on your own license.
Please note that you must install it manually (in GAC) before installing this package.


Three setting items have been added into your config file:

<configuration>
	<connectionStrings>
		<add name="$rootnamespace$.MainConnection" providerName="DDTek.Oracle" connectionString="Data Source=SAMPLEDB;Procedure Description Cache=false;Authentication Method=Client"/>
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