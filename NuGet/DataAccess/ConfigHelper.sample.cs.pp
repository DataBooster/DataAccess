using System.Data.Common;
using System.Configuration;

namespace $rootnamespace$.DataAccess
{
	public static partial class ConfigHelper
	{
		#region Properties
		private static DbProviderFactory _DbProviderFactory;
		public static DbProviderFactory DbProviderFactory
		{
			get { return _DbProviderFactory; }
		}

		private static string _ConnectionString;
		public static string ConnectionString
		{
			get { return _ConnectionString; }
		}

		private static string _DatabasePackage;
		public static string DatabasePackage
		{
			get { return _DatabasePackage; }
		}
		#endregion

		static ConfigHelper()
		{
			#region Setting key names defined in your config file
			const string connectionSettingKey = "$rootnamespace$.MainConnection";
			const string packageSettingKey = "$rootnamespace$.MainPackage";
			#endregion

			#region Default Initialization
			ConnectionStringSettings connSetting = ConfigurationManager.ConnectionStrings[connectionSettingKey];
			_DbProviderFactory = DbProviderFactories.GetFactory(connSetting.ProviderName);
			_ConnectionString = connSetting.ConnectionString;

			_DatabasePackage = ConfigurationManager.AppSettings[packageSettingKey];
			if (_DatabasePackage == null)
				_DatabasePackage = string.Empty;
			#endregion

			ConfigInit();
		}

		static partial void ConfigInit();
	}
}
