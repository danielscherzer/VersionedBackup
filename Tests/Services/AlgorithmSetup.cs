using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using VersionedCopy.Interfaces;

namespace VersionedCopyTests.Services
{
	public class AlgorithmSetup
	{
		protected readonly IDirectories dirs = new Dirs();
		protected readonly IReport nullReport = new NullReport();
		protected readonly CancellationToken token = new();

		protected void AssertEmptyDestination(VirtualFileSystem fileSystem)
		{
			var allDestDirs = fileSystem.EnumerateDirsRecursive(dirs.DestinationDirectory);
			Assert.AreEqual(1, allDestDirs.Count());
			Assert.AreEqual(0, fileSystem.EnumerateFiles(allDestDirs).Count());
		}
	}
}