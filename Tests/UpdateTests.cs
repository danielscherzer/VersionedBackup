using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using VersionedCopyTests.Services;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class UpdateTests
	{//todo: execute some tests for all algos
		[TestMethod()]
		public void RunSrcExistsTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			fileSystem.CreateDirectory(env.Options.SourceDirectory);
			Update.Run(env);
			Assert.IsTrue(fileSystem.ExistsDirectory(env.Options.DestinationDirectory));
		}

		[TestMethod()]
		public void RunCopyDirsTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			fileSystem.CreateDirectory(env.Options.SourceDirectory);
			var srcDirs = new string[] { "a", env.Options.SourceDirectory + "a", "b", "_" };
			foreach (var subDir in srcDirs) fileSystem.CreateDirectory(Path.Combine(env.Options.SourceDirectory, subDir));
			fileSystem.CreateDirectory(Path.Combine(env.Options.DestinationDirectory, srcDirs[0]));
			Update.Run(env);
			foreach (var subDir in srcDirs) Assert.IsTrue(fileSystem.ExistsDirectory(Path.Combine(env.Options.DestinationDirectory, subDir)));
		}


		[TestMethod()]
		public void RunCopyFilesTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			fileSystem.CreateDirectory(env.Options.SourceDirectory);
			var srcFiles = new string[] { "d", "b", "c", "z" };
			foreach (var file in srcFiles) fileSystem.CreateFile(Path.Combine(env.Options.SourceDirectory, file));
			Update.Run(env);
			foreach (var file in srcFiles) Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(env.Options.DestinationDirectory, file)));
		}

		[TestMethod()]
		public void RunCopyTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			fileSystem.CreateDirectory(env.Options.SourceDirectory);
			var srcDirs = new string[] { "a", env.Options.SourceDirectory + "a", "b", "c", "d" };
			foreach (var subDir in srcDirs) fileSystem.CreateDirectory(Path.Combine(env.Options.SourceDirectory, subDir));
			var srcFiles = new string[] { "x", "y", "z" };
			foreach (var file in srcFiles) fileSystem.CreateFile(Path.Combine(env.Options.SourceDirectory, file));

			Update.Run(env);
			FileSystemPart.AssertContains(fileSystem, env.Options.DestinationDirectory, srcFiles, srcDirs);
		}

		[TestMethod()]
		public void RunBigCopyTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			var part = new FileSystemPart(fileSystem, env.Options.SourceDirectory, 21);
			Update.Run(env);
			part.AssertContainsPart(env.Options.DestinationDirectory);
		}

		[TestMethod()]
		public void UpdateFromEmptyTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			fileSystem.CreateDirectory(env.Options.SourceDirectory);
			var part = new FileSystemPart(fileSystem, env.Options.DestinationDirectory, 21);

			Update.Run(env);
			//check all dst files/folders remain untoched
			part.AssertContainsPart(env.Options.DestinationDirectory);
		}

		[TestMethod()]
		public void UpdateTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			var partSrc = new FileSystemPart(fileSystem, env.Options.SourceDirectory, 21);
			var partDst = new FileSystemPart(fileSystem, env.Options.DestinationDirectory, 6, "x");

			Update.Run(env);
			partSrc.AssertContainsPart(env.Options.DestinationDirectory);
			partDst.AssertContainsPart(env.Options.DestinationDirectory);
		}


		[TestMethod()]
		public void UpdateNewerSrcTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			fileSystem.CreateDirectory(env.Options.SourceDirectory);
			var partDst = new FileSystemPart(fileSystem, env.Options.DestinationDirectory, 6, "x");
			var updatedFile = Path.Combine(env.Options.SourceDirectory, partDst.Files.First());
			fileSystem.CreateFile(updatedFile);
			fileSystem.UpdateFile(updatedFile);
			var updatedFileDst = Path.Combine(env.Options.DestinationDirectory, partDst.Files.First());

			Update.Run(env);
			partDst.AssertContainsPart(env.Options.DestinationDirectory);
			// check if same file after run
			Assert.IsFalse(fileSystem.HasChanged(updatedFile, updatedFileDst));
		}

		[TestMethod()]
		public void UpdateNewerDstTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			fileSystem.CreateDirectory(env.Options.SourceDirectory);
			var partDst = new FileSystemPart(fileSystem, env.Options.DestinationDirectory, 6, "x");
			var srcFile = Path.Combine(env.Options.SourceDirectory, partDst.Files.First());
			fileSystem.CreateFile(srcFile);
			var updatedFileDst = Path.Combine(env.Options.DestinationDirectory, partDst.Files.First());
			fileSystem.UpdateFile(updatedFileDst);

			Update.Run(env);
			partDst.AssertContainsPart(env.Options.DestinationDirectory);
			Assert.AreEqual(1, fileSystem.CompareAge(updatedFileDst, srcFile));
		}
	}
}