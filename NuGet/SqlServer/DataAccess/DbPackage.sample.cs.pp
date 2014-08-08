﻿using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Collections.Generic;
using DbParallel.DataAccess;
using DbParallel.DataAccess.Booster.SqlServer;

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

		#region Usage Examples
		// To turn off following sample code in DEBUG mode, just add NO_SAMPLE into the project properties ...
		// Project Properties Dialog -> Build -> General -> Conditional Compilation Symbols.
#if (DEBUG && !NO_SAMPLE)
		#region Some samples of using DbAccess class

		#region A sample of reader action
		public class SampleClassA
		{
			public string PropertyA { get; set; }
			public int PropertyB { get; set; }
			public decimal? PropertyC { get; set; }
			public SampleClassA()	// => ... <T> ... where T : new()
			{
				PropertyA = string.Empty;
				PropertyB = 0;
				PropertyC = 0m;
			}
			public SampleClassA(string a, int b, decimal? c)
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
					sampleClsList.Add(new SampleClassA(row.Field<string>("COL_A"), row.Field<int>("COL_B"), row.Field<decimal?>("COL_C")));
				});

			return sampleClsList;
		}
		#endregion

		#region A sample of specified fields mapping
		public class SampleClassB
		{
			public string PropertyA { get; set; }
			public int PropertyB { get; set; }
			public decimal? PropertyC { get; set; }
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
					// In order to allow Visual Studio Intellisense to be able to infer the type of map and t correctly
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
			public decimal? Column_Name3 { get; set; }
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

		#region A sample of Multi-ResultSets with auto/specified fields mapping
		internal static Tuple<List<SampleClassA>, List<SampleClassB>, List<SampleClassC>> ViewReport(this DbAccess dbAccess, DateTime date, int sessionId)
		{
			const string sp = "VIEW_REPORT";

			Tuple<List<SampleClassA>, List<SampleClassB>, List<SampleClassC>> resultTuple = new Tuple<List<SampleClassA>, List<SampleClassB>, List<SampleClassC>>(
				new List<SampleClassA>(), new List<SampleClassB>(), new List<SampleClassC>());

			dbAccess.ExecuteMultiReader(GetProcedure(sp), parameters =>
				{
					parameters.Add("inDate", date);
					parameters.Add("inSession", sessionId);
				}, resultSets =>
					{
						// Specified fields mapping example
						resultSets.Add(resultTuple.Item1,	// Put 1st ResultSet into resultTuple.Item1
							colMap =>
								{
									colMap.Add("COL_A", t => t.PropertyA);
									colMap.Add("COL_B", t => t.PropertyB);
									colMap.Add("COL_C", t => t.PropertyC);
								});

						// Full-automatic (case-insensitive) fields mapping examples
						resultSets.Add(resultTuple.Item2);   // Put 2nd ResultSet into resultTuple.Item2
						resultSets.Add(resultTuple.Item3);   // Put 3rd ResultSet into resultTuple.Item3
					}
			);

			return resultTuple;
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
					outRefreshInterval = parameters.AddOutput("outRefresh_Interval", DbType.Int32);
					outDegreeOfTaskParallelism = parameters.AddOutput("outParallelism_Degree", DbType.Byte);
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

		#region A sample of using SqlLauncher class
		public static SqlLauncher CreateSampleSqlLauncher()
		{
			return new SqlLauncher(ConfigHelper.ConnectionString, "schema.DestBigTable",
				/*	(Optional)
				 *	If the data source and the destination table have the same number of columns,
				 *	and the ordinal position of each source column within the data source matches the ordinal position
				 *	of the corresponding destination column, the ColumnMappings collection is unnecessary.
				 *	However, if the column counts differ, or the ordinal positions are not consistent,
				 *	you must use ColumnMappings to make sure that data is copied into the correct columns.
				 *	(this remark is excerpted from MSDN http://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlbulkcopy.columnmappings.aspx,
				 *	please see also http://msdn.microsoft.com/en-us/library/system.data.sqlclient.sqlbulkcopycolumnmappingcollection.aspx)
				 */	columnMappings =>
					{
						columnMappings.Add(/* 0, */"ProdID");
						columnMappings.Add(/* 1, */"ProdName");
						columnMappings.Add(/* 2, */"ProdPrice");
					}
			);
		}

		public static void AddSampleSqlRow(this SqlLauncher launcher, int col0, string col1, decimal col2)
		{
			launcher.Post(col0, col1, col2);
		}
		#endregion
#endif
		#endregion
	}
}
