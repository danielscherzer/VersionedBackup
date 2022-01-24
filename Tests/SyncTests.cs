using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using VersionedCopyTests.Services;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class SyncTests
	{
		[TestMethod()]
		public void SyncTest()
		{
			var fileSystem = new VirtualFileSystem();
			var env = AlgorithmTestSetup.Create(fileSystem);
			fileSystem.CreateDirectory(env.Options.SourceDirectory);
			fileSystem.CreateDirectory(env.Options.DestinationDirectory);
			var file1 = "1";
			fileSystem.CreateFile(Path.Combine(env.Options.SourceDirectory, file1));
			var file2 = "2";
			fileSystem.CreateFile(Path.Combine(env.Options.DestinationDirectory, file2));

			Sync.Run(env);
			Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(env.Options.SourceDirectory, file2)));
			Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(env.Options.DestinationDirectory, file1)));
		}
	}
}