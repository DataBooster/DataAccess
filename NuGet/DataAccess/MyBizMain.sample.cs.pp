#if DEBUG
using System;
using DbParallel.DataAccess;
using $rootnamespace$.DataAccess;

namespace $rootnamespace$
{
	class MyBizMain
	{
		private readonly DbAccess _MainDbAccess;

		public MyBizMain()
		{
			_MainDbAccess = DbPackage.CreateConnection();
		}

		public void MyTestProcess()
		{
			try
			{
				// ...
				var test1 = _MainDbAccess.GetSampleSetting("TestDomain");
				// ...
				var test2 = _MainDbAccess.LoadSampleObjByAction(test1.Item1, test1.Item2);
				// ...
				var test3 = _MainDbAccess.LoadSampleObjByMap(test1.Item1, test1.Item2);
				// ...
				var test4 = _MainDbAccess.LoadSampleObjAutoMap(test1.Item1, test1.Item2);
				// ...
			}
			catch (Exception e)
			{
				_MainDbAccess.LogSampleError(e.Source, e.Message);
			}
		}
	}
}
#endif
