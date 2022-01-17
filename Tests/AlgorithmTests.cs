using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using VersionedCopy.Interfaces;
using VersionedCopyTests.Services;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class AlgorithmTests
	{
		private readonly IDirectories dirs = new Dirs();
		private readonly IReport nullReport = new NullReport();
		private readonly CancellationToken token = new();

		[TestMethod()]
		public void RunEmptyTest()
		{
			var fileSystem = new VirtualFileSystem();
			Assert.ThrowsException<Exception>(() =>
			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token));
		}

		[TestMethod()]
		public void RunSrcExistsTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			Assert.IsTrue(fileSystem.ExistsDirectory(dirs.DestinationDirectory));
		}

		[TestMethod()]
		public void RunCopyDirsTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var srcDirs = new string[] {"a", dirs.SourceDirectory + "a", "b", "_" };
			foreach (var subDir in srcDirs) fileSystem.CreateDirectory(Path.Combine(dirs.SourceDirectory, subDir));
			fileSystem.CreateDirectory(Path.Combine(dirs.DestinationDirectory, srcDirs[0]));
			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			foreach (var subDir in srcDirs) Assert.IsTrue(fileSystem.ExistsDirectory(Path.Combine(dirs.DestinationDirectory, subDir)));
		}


		[TestMethod()]
		public void RunCopyFilesTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var srcFiles = new string[] { "d", "b", "c", "z" };
			foreach (var file in srcFiles) fileSystem.CreateFile(Path.Combine(dirs.SourceDirectory, file));
			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			foreach (var file in srcFiles) Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(dirs.DestinationDirectory, file)));
		}

		[TestMethod()]
		public void RunCopyTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var srcDirs = new string[] { "a", dirs.SourceDirectory + "a", "b", "c", "d" };
			foreach (var subDir in srcDirs) fileSystem.CreateDirectory(Path.Combine(dirs.SourceDirectory, subDir));
			var srcFiles = new string[] { "x", "y", "z" };
			foreach (var file in srcFiles) fileSystem.CreateFile(Path.Combine(dirs.SourceDirectory, file));
			
			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			AssertContains(fileSystem, dirs.DestinationDirectory, srcFiles, srcDirs);
		}

		[TestMethod()]
		public void RunBigCopyTest()
		{
			var fileSystem = new VirtualFileSystem();
			List<string> srcFiles = new();
			List<string> srcSubDirs = new();
			int relativeStart = dirs.SourceDirectory.Length + 1;
			fileSystem.RndFill(dirs.SourceDirectory, 21, "", path => srcSubDirs.Add(path[relativeStart..]), path => srcFiles.Add(path[relativeStart..]));

			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			AssertContains(fileSystem, dirs.DestinationDirectory, srcFiles, srcSubDirs);
		}

		[TestMethod()]
		public void MirrorEmptyTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			List<string> dstFiles = new();
			List<string> dstSubDirs = new();
			int relativeStart = dirs.DestinationDirectory.Length + 1;
			fileSystem.RndFill(dirs.DestinationDirectory, 21, "", path => dstSubDirs.Add(path[relativeStart..]), path => dstFiles.Add(path[relativeStart..]));

			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			AssertEmptyDestination(fileSystem);
			//check all dst files/folders are moved to old
			AssertContains(fileSystem, dirs.OldFilesFolder, dstFiles, dstSubDirs);
		}

		[TestMethod()]
		public void RunBigMirrorTest()
		{
			var fileSystem = new VirtualFileSystem();
			List<string> srcFiles = new();
			List<string> srcSubDirs = new();
			int relativeStart = dirs.SourceDirectory.Length + 1;
			fileSystem.RndFill(dirs.DestinationDirectory, 4); // add some files/dirs at destination that should be removed
			fileSystem.RndFill(dirs.SourceDirectory, 21, "xx", path => srcSubDirs.Add(path[relativeStart..]), path => srcFiles.Add(path[relativeStart..]));

			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			
			AssertContains(fileSystem, dirs.DestinationDirectory, srcFiles, srcSubDirs);
			//TODO: check if no addtional files exist
			foreach (var file in srcFiles) fileSystem.DeleteFile(Path.Combine(dirs.DestinationDirectory, file));
			foreach (var subDir in srcSubDirs) fileSystem.DeleteDir(Path.Combine(dirs.DestinationDirectory, subDir));
			AssertEmptyDestination(fileSystem);
		}

		private static void AssertContains(VirtualFileSystem fileSystem, string root, IEnumerable<string> srcFiles, IEnumerable<string> srcSubDirs)
		{
			foreach (var file in srcFiles) Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(root, file)));
			foreach (var subDir in srcSubDirs) Assert.IsTrue(fileSystem.ExistsDirectory(Path.Combine(root, subDir)));
		}

		private void AssertEmptyDestination(VirtualFileSystem fileSystem)
		{
			var allDestDirs = fileSystem.EnumerateDirsRecursive(dirs.DestinationDirectory);
			Assert.AreEqual(1, allDestDirs.Count());
			Assert.AreEqual(0, fileSystem.EnumerateFiles(allDestDirs).Count());
		}
	}
}