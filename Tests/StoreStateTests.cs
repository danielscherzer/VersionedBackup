using Microsoft.VisualStudio.TestTools.UnitTesting;
using static VersionedCopy.Tests.FileSystemHelper;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class StoreStateTests
	{
		[TestMethod()]
		public void RunTest()
		{
			//for(int i = 0; i < 100; ++i)
			//{
			//	Stopwatch stopwatch = Stopwatch.StartNew();
			//StoreState.Run(@"d:\daten", @"d:\test.json", Enumerable.Empty<string>(), Enumerable.Empty<string>());
			//	//stopwatch.Benchmark("store state run");
			//}
		}

		//[DataTestMethod()]
		//public void StoreStateTest()
		//{
		//	var list = new string[] { "F1", "F2", "a\\F1", "a\\F2", "a\\F3", "a\\F4", "b\\", "c\\", "a\\b\\c\\d" };
		//	var src = ToPath("src");
		//	var dst = ToPath("test.json");
		//	foreach (var item in list) Create(src, item);

		//	Program.Main(new string[] { "storeState", src, dst });
		//}

		[TestCleanup]
		public void TestCleanup()
		{
			Cleanup();
		}
	}
}