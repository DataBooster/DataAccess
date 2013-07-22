using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using DbParallel.DataAccess;

namespace $rootnamespace$.DataAccess
{
	public static partial class DbPackage
	{
		#region Initialization
		static DbPackage()
		{
		//	DbAccess.DefaultCommandType = CommandType.StoredProcedure;
		}

		public static DbAccess CreateConnection()
		{
			return new DbAccess(ConfigHelper.DbProviderFactory, ConfigHelper.ConnectionString);
		}

		private static string GetProcedure(string sp)
		{
			return ConfigHelper.DatabasePackage + sp;
		}
		#endregion

#if DEBUG
		#region Some samples for using DbAccess class

		#region A sample of reader action
		public class SampleClassA
		{
			public string PropertyA { get; set; }
			public int PropertyB { get; set; }
			public decimal PropertyC { get; set; }
			public SampleClassA(string a, int b, decimal c)
			{
				PropertyA = a;
				PropertyB = b;
				PropertyC = c;
			}
		}

		internal static List<SampleClassA> LoadSampleObjByAction(this DbAccess dbAccess, string area, int grpId)
		{
			const string sp = "GET_SAMPLE_OBJ";
			List<SampleClassA> sampleClsList = new List<SampleClassA>();

			dbAccess.ExecuteReader(GetProcedure(sp), parameters =>
				{
					parameters.Add("inArea", area);
					parameters.Add("inGrp_Id", grpId);
				},
				row =>
				{
					// ...
					sampleClsList.Add(new SampleClassA(row.Field<string>("COL_A"), row.Field<int>("COL_B"), row.Field<decimal>("COL_C")));
				});

			return sampleClsList;
		}
		#endregion

		#region A sample of specified fields mapping
		public class SampleClassB
		{
			public string PropertyA { get; set; }
			public int PropertyB { get; set; }
			public decimal PropertyC { get; set; }
		//	public SampleClassB() { }	// Implicitly or Explicitly
		}

		internal static IEnumerable<SampleClassB> LoadSampleObjByMap(this DbAccess dbAccess, string area, int grpId)
		{
			const string sp = "GET_SAMPLE_OBJ";

			return dbAccess.ExecuteReader<SampleClassB>(GetProcedure(sp), parameters =>
				{
					parameters.Add("inArea", area);
					parameters.Add("inGrp_Id", grpId);
				},
				map =>
				{
					// Known issue: Visual Studio Intellisense may not able to infer the type of map and t correctly
					// Please make sure that SampleClassB class has a parameterless constructor or default constructor implicitly
					map.Add("COL_A", t => t.PropertyA);
					map.Add("COL_B", t => t.PropertyB);
					map.Add("COL_C", t => t.PropertyC);
				});
		}
		#endregion

		#region A sample of auto fields mapping
		public class SampleClassC
		{
			public string Column_Name1 { get; set; }
			public int Column_Name2 { get; set; }
			public decimal Column_Name3 { get; set; }
			public bool Column_Name4 { get; set; }
		//	public SampleClassC() { }	// Implicitly or Explicitly
		}

		internal static IEnumerable<SampleClassC> LoadSampleObjAutoMap(this DbAccess dbAccess, string area, int grpId)
		{
			const string sp = "GET_SAMPLE_ITEM";

			return dbAccess.ExecuteReader<SampleClassC>(GetProcedure(sp), parameters =>
				{
					parameters.Add("inArea", area);
					parameters.Add("inGrp_Id", grpId);
				});
		}
		#endregion

		#region A sample of output parameter
		internal static Tuple<string, int, byte> GetSampleSetting(this DbAccess dbAccess, string sampleDomain)
		{
			const string sp = "GET_SAMPLE_SETTING";
			DbParameter outStartupMode = null;
			DbParameter outRefreshInterval = null;
			DbParameter outDegreeOfTaskParallelism = null;

			dbAccess.ExecuteNonQuery(GetProcedure(sp), parameters =>
				{
					parameters.Add("inSample_Domain", sampleDomain);
					outStartupMode = parameters.AddOutput("outStartup_Mode", 32);
					outRefreshInterval = parameters.AddOutput("outRefresh_Interval");
					outDegreeOfTaskParallelism = parameters.AddOutput("outParallelism_Degree");
				});

			return Tuple.Create(outStartupMode.Parameter<string>(),
				outRefreshInterval.Parameter<int>(),
				outDegreeOfTaskParallelism.Parameter<byte>());
		}
		#endregion

		#region A sample of non-return
		public static void LogSampleError(this DbAccess dbAccess, string strReference, string strMessage)
		{
			const string sp = "LOG_SAMPLE_ERROR";

			dbAccess.ExecuteNonQuery(GetProcedure(sp), parameters =>
				{
					parameters.Add("inReference", strReference);
					parameters.Add("inMessage", strMessage);
				});
		}
		#endregion

		#endregion

		// Please see http://databooster.codeplex.com for more samples
#endif
	}
}
