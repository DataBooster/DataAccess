#if DEBUG
using System;
using DbParallel.DataAccess;
using $rootnamespace$.DataAccess;

namespace $rootnamespace$
{
	class MyBizMain
	{
		public void MyTestAccess()
		{
			using (DbAccess db = DbPackage.CreateConnection())
			{
				// ...
				var test1 = db.GetSampleSetting("TestDomain");
				// ...
				var test2 = db.LoadSampleObjByAction(test1.Item1, test1.Item2);
				// ...
				using (DbTransactionScope tran = db.NewTransactionScope())	// Start a transaction
				{
					var test3 = db.LoadSampleObjByMap(test1.Item1, test1.Item2);
					// ...
					db.LogSampleError(e.Source, e.Message);
					// ...
					tran.Complete();
				}	// Exit (Commit) the transaction
				// ...
				var test4 = db.LoadSampleObjAutoMap(test1.Item1, test1.Item2);
				// ...
			}
		}
	}
}
#endif
