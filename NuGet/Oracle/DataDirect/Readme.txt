Installation Notes:

This edition of library requires "Progress DataDirect Connect ADO.NET PROVIDER FOR ORACLE"
(http://www.datadirect.com/products/net/net-for-oracle/index.html) on your own license.
Please note that you must install it manually (in GAC) before installing this edition.


Two setting items have been added into your config file:

<configuration>
	<connectionStrings>
		<add name="$rootnamespace$.MainConnection" providerName="DDTek.Oracle" connectionString="TNSNames File=C:\Oracle\product\11.2.0\dbhome_1\NETWORK\ADMIN\tnsnames.ora;Data Source=SAMPLEDB;Authentication Method=Client"/>
	</connectionStrings>
	<appSettings>
		<add key="$rootnamespace$.MainPackage" value="SCHEMA.PACKAGE." />
	</appSettings>
</configuration>

Please update them as your practical environment.

And three *.sample.cs files have been copied into your project\DataAccess folder for quick starts.


See Also:
http://databooster.codeplex.com