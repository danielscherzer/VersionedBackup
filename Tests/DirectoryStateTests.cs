using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;
using VersionedCopy.Services;
using static VersionedCopy.Tests.FileSystemHelper;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class DirectoryStateTests
	{
		[TestMethod()]
		public void CreateTest()
		{
			var stopwatch = Stopwatch.StartNew();
			var state = DirectoryState.Create(@"d:\daten", new string[] { ".vs\\", "bin\\", "obj\\", "TestResults\\" }, Enumerable.Empty<string>());
			//File.WriteAllText(@"d:\daten_state.json", JsonConvert.SerializeObject(state, Formatting.Indented));
			stopwatch.Benchmark("save");
			var state2 = DirectoryState.Create(@"d:\daten", new string[] { ".vs\\", "bin\\", "obj\\" }, Enumerable.Empty<string>());
			//File.WriteAllText(@"d:\daten_state2.json", JsonConvert.SerializeObject(state2, Formatting.Indented));
		}

		[TestCleanup]
		public void TestCleanup() => Cleanup();
	}
}