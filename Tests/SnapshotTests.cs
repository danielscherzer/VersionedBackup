using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VersionedCopy.Services;
using static VersionedCopy.Tests.FileSystemHelper;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class SnapshotTests
	{
		[TestMethod()]
		public void CreateTest()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();
			var state = Snapshot.Create(@"d:\daten", new string[] { ".vs\\", "bin\\", "obj\\", "TestResults\\" }, Enumerable.Empty<string>());
			stopwatch.Benchmark("create");
			var json = JsonConvert.SerializeObject(state, Formatting.Indented);
			File.WriteAllText(Path.Combine("d:", $"snapshot {DateTime.Now:yyyy-MM-dd_HHmmss}.json"), json);
			var state2 = JsonConvert.DeserializeObject<Snapshot>(json);

			var stateOld = JsonConvert.DeserializeObject<Snapshot>(File.ReadAllText(@"D:\snapshot 2022-02-24_140801.json"));
			stopwatch.Benchmark("save");
			var diff = new SnapshotDiff(stateOld, state2);
			stopwatch.Benchmark("diff");
		}

		[TestMethod()]
		public void SaveTest()
		{
			var list = new string[] { "F1", "F2", "a\\F1", "a\\F2", "a\\F3", "a\\F4", "b\\", "c\\", "a\\b\\c\\d" };
			var src = ToPath("src");
			var dst = ToPath("test.json");
			foreach (var item in list) Create(src, item);

			Program.Main(new string[] { "snapshot", src, dst });
		}

		[TestCleanup]
		public void TestCleanup() => Cleanup();
	}
}