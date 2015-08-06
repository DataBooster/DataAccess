http://www.nuget.org/packages/DataBooster.Oracle.Managed
Installation Notes:

A dependency NuGet package "Oracle.ManagedDataAccess" - Official Oracle ODP.NET, Managed Driver
http://www.nuget.org/packages/Oracle.ManagedDataAccess has also been installed automatically.


Four setting items have been added into your config file:

<configuration>
	<connectionStrings>
		<add name="$rootnamespace$.MainConnection" providerName="Oracle.ManagedDataAccess.Client" connectionString="Data Source=SAMPLEDB;User Id=/"/>
		<add name="$rootnamespace$.AuxConnection" providerName="System.Data.SqlClient" connectionString="Data Source=.\SQLEXPRESS;Initial Catalog=SAMPLEDB;Integrated Security=SSPI" />
	</connectionStrings>
	<appSettings>
		<add key="$rootnamespace$.MainPackage" value="SCHEMA.PACKAGE." />
	</appSettings>
	<oracle.manageddataaccess.client>
		<version number="*">
			<settings>
				<!-- Set this path if you are using TNS aliases as connection strings -->
				<setting name="TNS_ADMIN" value="(ORACLE_HOME)\network\admin"/>	<!-- E.g. "c:\oracle\product\11.2.0\client_1\network\admin" -->
				<!-- Instead you can use "SERVER_NAME:PORT/SERVICE_NAME" as your data source -->
			</settings>
		</version>
	</oracle.manageddataaccess.client>
</configuration>

Please update them as your practical environment.

And three *.sample.cs files have been copied into your project\DataAccess folder for quick starts.


Open Source:
http://databooster.codeplex.com