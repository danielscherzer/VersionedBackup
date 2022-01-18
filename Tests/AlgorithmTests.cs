using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
			FileSystemPart.AssertContains(fileSystem, dirs.DestinationDirectory, srcFiles, srcDirs);
		}

		[TestMethod()]
		public void RunBigCopyTest()
		{
			var fileSystem = new VirtualFileSystem();
			var part = new FileSystemPart(fileSystem, dirs.SourceDirectory, 21);
			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			part.AssertContainsPart(dirs.DestinationDirectory);
		}

		[TestMethod()]
		public void MirrorEmptyTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var part = new FileSystemPart(fileSystem, dirs.DestinationDirectory, 21);
			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			AssertEmptyDestination(fileSystem);
			//check all dst files/folders are moved to old
			part.AssertContainsPart(dirs.OldFilesFolder);
		}

		[TestMethod()]
		public void RunBigMirrorTest()
		{
			var fileSystem = new VirtualFileSystem();
			var part = new FileSystemPart(fileSystem, dirs.SourceDirectory, 21, "x");
			fileSystem.RndFill(dirs.DestinationDirectory, 4); // add some files/dirs at destination that should be removed

			Algorithms.Run(new TestOptions(dirs), nullReport, fileSystem, token);

			part.AssertContainsPart(dirs.DestinationDirectory);
			//TODO: check if no addtional files exist
			foreach (var file in part.Files) fileSystem.DeleteFile(Path.Combine(dirs.DestinationDirectory, file));
			foreach (var subDir in part.SubDirs) fileSystem.DeleteDir(Path.Combine(dirs.DestinationDirectory, subDir));
			AssertEmptyDestination(fileSystem);
		}

		[TestMethod()]
		public void MirrorNewerDstTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var partDst = new FileSystemPart(fileSystem, dirs.DestinationDirectory, 6, "x");
			var srcFile = Path.Combine(dirs.SourceDirectory, partDst.Files.First());
			fileSystem.CreateFile(srcFile);
			var updatedFileDst = Path.Combine(dirs.DestinationDirectory, partDst.Files.First());
			fileSystem.UpdateFile(updatedFileDst);

			var options = new TestOptions(dirs) { Mode = AlgoMode.Mirror };
			Algorithms.Run(options, nullReport, fileSystem, token);
			Assert.IsFalse(fileSystem.HasChanged(updatedFileDst, srcFile));
			var updatedFileOld = Path.Combine(dirs.OldFilesFolder, partDst.Files.First());
			Assert.IsTrue(fileSystem.HasChanged(updatedFileDst, updatedFileOld));
		}

		[TestMethod()]
		public void UpdateFromEmptyTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var part = new FileSystemPart(fileSystem, dirs.DestinationDirectory, 21);

			var options = new TestOptions(dirs) { Mode = AlgoMode.Update };
			Algorithms.Run(options, nullReport, fileSystem, token);
			//check all dst files/folders remain untoched
			part.AssertContainsPart(dirs.DestinationDirectory);
		}

		[TestMethod()]
		public void UpdateTest()
		{
			var fileSystem = new VirtualFileSystem();
			var partSrc = new FileSystemPart(fileSystem, dirs.SourceDirectory, 21);
			var partDst = new FileSystemPart(fileSystem, dirs.DestinationDirectory, 6, "x");

			var options = new TestOptions(dirs) { Mode = AlgoMode.Update };
			Algorithms.Run(options, nullReport, fileSystem, token);
			partSrc.AssertContainsPart(dirs.DestinationDirectory);
			partDst.AssertContainsPart(dirs.DestinationDirectory);
		}

		[TestMethod()]
		public void UpdateNewerSrcTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var partDst = new FileSystemPart(fileSystem, dirs.DestinationDirectory, 6, "x");
			var updatedFile = Path.Combine(dirs.SourceDirectory, partDst.Files.First());
			fileSystem.CreateFile(updatedFile);
			fileSystem.UpdateFile(updatedFile);
			var updatedFileDst = Path.Combine(dirs.DestinationDirectory, partDst.Files.First());

			var options = new TestOptions(dirs) { Mode = AlgoMode.Update };
			Algorithms.Run(options, nullReport, fileSystem, token);
			partDst.AssertContainsPart(dirs.DestinationDirectory);
			// check if same file after run
			Assert.IsFalse(fileSystem.HasChanged(updatedFile, updatedFileDst));

		}

		[TestMethod()]
		public void UpdateNewerDstTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var partDst = new FileSystemPart(fileSystem, dirs.DestinationDirectory, 6, "x");
			var srcFile = Path.Combine(dirs.SourceDirectory, partDst.Files.First());
			fileSystem.CreateFile(srcFile);
			var updatedFileDst = Path.Combine(dirs.DestinationDirectory, partDst.Files.First());
			fileSystem.UpdateFile(updatedFileDst);

			var options = new TestOptions(dirs) { Mode = AlgoMode.Update };
			Algorithms.Run(options, nullReport, fileSystem, token);
			partDst.AssertContainsPart(dirs.DestinationDirectory);
			Assert.IsTrue(fileSystem.IsNewer(updatedFileDst, srcFile));
		}

		private void AssertEmptyDestination(VirtualFileSystem fileSystem)
		{
			var allDestDirs = fileSystem.EnumerateDirsRecursive(dirs.DestinationDirectory);
			Assert.AreEqual(1, allDestDirs.Count());
			Assert.AreEqual(0, fileSystem.EnumerateFiles(allDestDirs).Count());
		}
	}
}