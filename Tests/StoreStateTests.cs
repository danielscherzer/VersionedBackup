using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using static VersionedCopy.Tests.FileSystemHelper;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class StoreStateTests
	{
		[TestMethod()]
		public void CreateTest()
		{
			var state = StoreState.Create2(@"d:\daten", Enumerable.Empty<string>(), Enumerable.Empty<string>());
			//File.WriteAllText(@"d:\daten_state.json", JsonConvert.SerializeObject(state, Formatting.Indented));

			var state2 = StoreState.Create2(@"d:\daten", new string[] { ".vs\\", "bin\\", "obj\\" }, Enumerable.Empty<string>());
			//File.WriteAllText(@"d:\daten_state2.json", JsonConvert.SerializeObject(state2, Formatting.Indented));
		}

		[TestMethod()]
		public void StoreStateTest()
		{
			var list = new string[] { "F1", "F2", "a\\F1", "a\\F2", "a\\F3", "a\\F4", "b\\", "c\\", "a\\b\\c\\d" };
			var src = ToPath("src");
			var dst = ToPath("test.json");
			foreach (var item in list) Create(src, item);

			Program.Main(new string[] { "storeState", src, dst });
		}

		[TestCleanup]
		public void TestCleanup()
		{
			Cleanup();
		}
	}
}