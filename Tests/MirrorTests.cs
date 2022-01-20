using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using VersionedCopyTests.Services;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class MirrorTests : AlgorithmSetup
	{
		[TestMethod()]
		public void MirrorEmptyTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var part = new FileSystemPart(fileSystem, dirs.DestinationDirectory, 21);
			_ = new Mirror(new TestOptions(dirs), nullReport, fileSystem, token);
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

			_ = new Mirror(new TestOptions(dirs), nullReport, fileSystem, token);

			part.AssertContainsPart(dirs.DestinationDirectory);
			//Check if no addtional files exist
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

			_ = new Mirror(new TestOptions(dirs), nullReport, fileSystem, token);
			Assert.IsFalse(fileSystem.HasChanged(updatedFileDst, srcFile));
			var updatedFileOld = Path.Combine(dirs.OldFilesFolder, partDst.Files.First());
			Assert.IsTrue(fileSystem.HasChanged(updatedFileDst, updatedFileOld));
		}
	}
}