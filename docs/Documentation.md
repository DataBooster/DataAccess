# FAQ

## Configuration
* **Q**: My database connection doesn't work after I updated the NuGet package? It worked fine before.
* **A**: Every time NuGet updates the package, it just appends relevant elements again with default TODO attributes into your configuration file, then the later duplicate key hides your working attributes.
: Please remove those appended duplicate elements from your configuration file after NuGet updates the package.
: Alternatively, if you place the NuGet package in a separate Class Library (dll) project instead of in the main/startable project, NuGet will only touch the useless configuration file in that non-startable project. So that you don't worry about losing configuration.

* **Q**: I only need a connection to Oracle, however NuGet package always adds an extra SQL Server connection (AuxConnection) into my project. How can I turn it off?
* **A**:
: **Method 1** - Remove the **AuxConnection** item from the <connectionStrings> section in your working configuration.
: **Method 2** - Override the **_AuxConnectionSettingKey** (member of partial class ConfigHelper) as null or empty string in your own implementation of the partial method **OnInitializing()**.

* **Q**: How to add more different connections into my project?
* **A**: A recommended approach:
## Add more ConnectionStrings into your working configuration; and
## Extend new connections in your separate implementation _(e.g. ConfigHelper.partial.cs)_ of the partial class ConfigHelper;
## Also extend new connections in _DbPackage.partial.cs_.

* **Q**: NuGet adds a **MainPackage** element into the <appSettings> section of my configuration file, with some scaffolding for it. What's the use of it?
* **A**: The scaffold tend to provide convenience for **Package Based** (Oracle) or **Stored Procedure Based** (SQL Server) style database development. Please ignore it if you don't like this style.
	* For **Oracle** - the **MainPackage** config item just preassigns the full name (format: "_SCHEMA.PACKAGE**.**_") of the database package you mainly use in your project. For compatibility with SQL Server, please don't forget to put a dot (.) at the end of package name. Actually you can see the scaffolding (the method _string GetProcedure(string sp)_ in the class DbPackage) will concatenate this string with every Stored Procedure name simply before send request to database.
	* For **SQL Server** - the **MainPackage** config item represents the prefix (format: "_SCHEMA.PACKAGE**_**_") of stored procedures you mostly use in your project. Since SQL Server does not have a concept of **Package** in database programming, Usually naming convention is used to organize stored procedures to achieve a similar purpose as package.

* **Q**: The static global variable **DbAccess.DefaultCommandType** defaults to **CommandType.StoredProcedure**, How can I use dynamic SQL?
* **A**: 
	* If your codeing mostly is _dynamic SQL based style_ - embedding the whole SQL statement (such as "_SELECT * FROM ..._") inside your client side .Net code, You can set the static global variable _DbAccess.DefaultCommandType = **CommandType.Text**_ in your initialization.
	* Or if you just need some dynamic SQL occasionally, You can use those longer **overloads** of DbAccess.ExecuteReader or DbAccess.ExecuteMultiReader with commandType parameter.

## Parameters
* **Q**: When we add parameters by using DbParameterBuilder, in most cases we do not need to specify the DbType and Size, but sometimes needs, what's the criteria?
* **A**: Basically for **Input** and **InputOut** kinds of parameters, the DbType and Size can be inferred from the .NET Framework type of the **Value** of the Parameter object you passed in. So they don't need to be specified in _Add(...)_ methods.
: But for **Output** and **ReturnValue** kinds of parameters, since none value to pass into, there's no way to infer the DbType and Size, so they need to be specified in _AddOutput(...)_ method.
: For **ReturnValue** (use _AddReturn(...)_ method), you can be lazy only if the return value is a Int32 type. In addition, return value parameter must be the first parameter added to the Parameters collection.

* **Q**: Is it necessary to explicitly call _AddRefCursor(...)_ for Oracle output REF CURSOR parameters?
* **A**: No, it is not necessary. For Oracle connection, **DbAccess.ExecuteReader** overloads automatically add a output REF CURSOR parameter if you don't add any REF CURSOR parameter explicitly; and **DbAccess.ExecuteMultiReader** overloads can infer how many output REF CURSOR parameters based on the ResultSet-Mapping you specified.

* **Q**: Must I use the "**@**" prefix as named parameters for SqlClient data providers?
* **A**: Although public documents on MSDN specify that SQL Server Data Provider uses named parameters in the format "@ParameterName", however the format "ParameterName" without "@" prefix is also serviceable in practice. The simpler format allows one code to work for both SQL Server and Oracle compatibly.
