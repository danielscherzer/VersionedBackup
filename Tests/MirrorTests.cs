using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using VersionedCopyTests.Services;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class MirrorTests
	{
		[TestMethod()]
		public void MirrorEmptyTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			env.FileSystem.CreateDirectory(env.Options.SourceDirectory);
			var part = new FileSystemPart(fileSystem, env.Options.DestinationDirectory, 21);
			Mirror.Run(env);
			env.AssertEmptyDestination();
			//check all dst files/folders are moved to old
			part.AssertContainsPart(env.Options.OldFilesFolder);
		}

		[TestMethod()]
		public void RunBigMirrorTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			var part = new FileSystemPart(fileSystem, env.Options.SourceDirectory, 21, "x");
			fileSystem.RndFill(env.Options.DestinationDirectory, 4); // add some files/dirs at destination that should be removed

			Mirror.Run(env);

			part.AssertContainsPart(env.Options.DestinationDirectory);
			//Check if no addtional files exist
			foreach (var file in part.Files) fileSystem.DeleteFile(Path.Combine(env.Options.DestinationDirectory, file));
			foreach (var subDir in part.SubDirs) fileSystem.DeleteDir(Path.Combine(env.Options.DestinationDirectory, subDir));
			env.AssertEmptyDestination();
		}

		[TestMethod()]
		public void MirrorNewerDstTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			fileSystem.CreateDirectory(env.Options.SourceDirectory);
			var partDst = new FileSystemPart(fileSystem, env.Options.DestinationDirectory, 6, "x");
			var srcFile = Path.Combine(env.Options.SourceDirectory, partDst.Files.First());
			fileSystem.CreateFile(srcFile);
			var updatedFileDst = Path.Combine(env.Options.DestinationDirectory, partDst.Files.First());
			fileSystem.UpdateFile(updatedFileDst);

			Mirror.Run(env);

			Assert.IsFalse(fileSystem.HasChanged(updatedFileDst, srcFile));
			var updatedFileOld = Path.Combine(env.Options.OldFilesFolder, partDst.Files.First());
			Assert.IsTrue(fileSystem.HasChanged(updatedFileDst, updatedFileOld));
		}
	}
}