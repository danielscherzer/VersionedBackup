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

			var oldStateFile = @"D:\snapshot 2022-02-26_103142.json";
			if (File.Exists(oldStateFile))
			{
				var stateOld = JsonConvert.DeserializeObject<Snapshot>(File.ReadAllText(oldStateFile));
				stopwatch.Benchmark("save");
				var diff = new SnapshotDiff(stateOld, state2);
				stopwatch.Benchmark("diff");
				SavePatch(diff, @"d:\diff.zip");
			}
		}

		//TODO: we can only delete files for wich we have a history that shows a delete otherwise it could be a other new

		private void SavePatch(SnapshotDiff diff, string fileName)
		{
			if(diff.OtherNewerFiles.Any())
			{
				throw new Exception("Conflicting files found. Update first.");
			}
			foreach(var newFile in diff.NewFiles)
			{

			}
			foreach (var updatedFile in diff.UpdatedFiles)
			{

			}
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