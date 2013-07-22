Installation Notes:

This edition of library requires "Oracle Data Provider for .NET (ODP.NET)"
(http://www.oracle.com/technetwork/topics/dotnet/index-085163.html) on your own license.
Please note that you must install it manually (in GAC) before installing this edition.


Two setting items have been added into your config file:

<configuration>
	<connectionStrings>
		<add name="$rootnamespace$.MainConnection" providerName="Oracle.DataAccess.Client" connectionString="Data Source=SAMPLEDB;Integrated Security=yes;"/>
	</connectionStrings>
	<appSettings>
		<add key="$rootnamespace$.MainPackage" value="SCHEMA.PACKAGE." />
	</appSettings>
</configuration>

Please update them as your practical environment.

And three *.sample.cs files have been copied into your project\DataAccess folder for quick starts.


See Also:
http://databooster.codeplex.com