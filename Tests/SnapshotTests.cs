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
			var state = Snapshot.Create(@"d:\daten", new string[] { ".vs\\", "bin\\", "obj\\", "TestResults\\" }, Enumerable.Empty<string>(), new System.Threading.CancellationToken());
			stopwatch.Benchmark("create");
			var json = JsonConvert.SerializeObject(state, Formatting.Indented);
			File.WriteAllText(Path.Combine("d:", $"snapshot {DateTime.Now:yyyy-MM-dd_HHmmss}.json"), json);
			var stateMine = JsonConvert.DeserializeObject<Snapshot>(json);

			var oldStateFile = @"D:\snapshot 2022-02-27_095839.json";
			if (File.Exists(oldStateFile))
			{
				var stateOld = JsonConvert.DeserializeObject<Snapshot>(File.ReadAllText(oldStateFile));
				stopwatch.Benchmark("save");
			}
		}

		[TestMethod()]
		public void SaveTest()
		{
			var list = new string[] { "F1", "F2", "a\\F1", "a\\F2", "a\\F3", "a\\F4", "b\\", "c\\", "a\\b\\c\\d" };
			var src = ToPath("src");
			foreach (var item in list) Create(src, item);

			Snapshot snapShot = Snapshot.Create(src, Enumerable.Empty<string>(), Enumerable.Empty<string>(), new System.Threading.CancellationToken());
			foreach(var item in list)
			{
				Assert.IsTrue(snapShot.Entries.ContainsKey(item));
			}
		}

		[TestCleanup]
		public void TestCleanup() => Cleanup();
	}
}