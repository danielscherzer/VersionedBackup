using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using VersionedCopyTests.Services;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class UpdateTests : AlgorithmSetup
	{//todo: execute some tests for all algos
		[TestMethod()]
		public void RunSrcExistsTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			_ = new Update(new TestOptions(dirs), nullReport, fileSystem, token);
			Assert.IsTrue(fileSystem.ExistsDirectory(dirs.DestinationDirectory));
		}

		[TestMethod()]
		public void RunCopyDirsTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var srcDirs = new string[] { "a", dirs.SourceDirectory + "a", "b", "_" };
			foreach (var subDir in srcDirs) fileSystem.CreateDirectory(Path.Combine(dirs.SourceDirectory, subDir));
			fileSystem.CreateDirectory(Path.Combine(dirs.DestinationDirectory, srcDirs[0]));
			_ = new Update(new TestOptions(dirs), nullReport, fileSystem, token);
			foreach (var subDir in srcDirs) Assert.IsTrue(fileSystem.ExistsDirectory(Path.Combine(dirs.DestinationDirectory, subDir)));
		}


		[TestMethod()]
		public void RunCopyFilesTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var srcFiles = new string[] { "d", "b", "c", "z" };
			foreach (var file in srcFiles) fileSystem.CreateFile(Path.Combine(dirs.SourceDirectory, file));
			_ = new Update(new TestOptions(dirs), nullReport, fileSystem, token);
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

			_ = new Update(new TestOptions(dirs), nullReport, fileSystem, token);
			FileSystemPart.AssertContains(fileSystem, dirs.DestinationDirectory, srcFiles, srcDirs);
		}

		[TestMethod()]
		public void RunBigCopyTest()
		{
			var fileSystem = new VirtualFileSystem();
			var part = new FileSystemPart(fileSystem, dirs.SourceDirectory, 21);
			_ = new Update(new TestOptions(dirs), nullReport, fileSystem, token);
			part.AssertContainsPart(dirs.DestinationDirectory);
		}

		[TestMethod()]
		public void UpdateFromEmptyTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var part = new FileSystemPart(fileSystem, dirs.DestinationDirectory, 21);

			_ = new Update(new TestOptions(dirs), nullReport, fileSystem, token);
			//check all dst files/folders remain untoched
			part.AssertContainsPart(dirs.DestinationDirectory);
		}

		[TestMethod()]
		public void UpdateTest()
		{
			var fileSystem = new VirtualFileSystem();
			var partSrc = new FileSystemPart(fileSystem, dirs.SourceDirectory, 21);
			var partDst = new FileSystemPart(fileSystem, dirs.DestinationDirectory, 6, "x");

			_ = new Update(new TestOptions(dirs), nullReport, fileSystem, token);
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

			_ = new Update(new TestOptions(dirs), nullReport, fileSystem, token);
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

			_ = new Update(new TestOptions(dirs), nullReport, fileSystem, token);
			partDst.AssertContainsPart(dirs.DestinationDirectory);
			Assert.AreEqual(1, fileSystem.CompareAge(updatedFileDst, srcFile));
		}
	}
}