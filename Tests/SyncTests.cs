using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using VersionedCopyTests.Services;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class SyncTests : AlgorithmSetup
	{
		[TestMethod()]
		public void SyncTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var file1 = "1";
			fileSystem.CreateFile(Path.Combine(dirs.SourceDirectory, file1));
			var file2 = "2";
			fileSystem.CreateFile(Path.Combine(dirs.DestinationDirectory, file2));

			_ = new Sync(new TestOptions(dirs), nullReport, fileSystem, token);
			Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(dirs.SourceDirectory, file2)));
			Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(dirs.DestinationDirectory, file1)));
		}
	}
}