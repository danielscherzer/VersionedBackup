using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using VersionedCopy.Services;

namespace VersionedCopy.Tests.Services
{
	public static class AlgorithmTestSetup
	{
		public static AlgorithmEnv Create(VirtualFileSystem fs) => new(
			new Options("src", "dst", "old", Enumerable.Empty<string>(), Enumerable.Empty<string>(), false),
			new NullReport(), fs, new CancellationToken());

		public static void AssertEmptyDestination(this AlgorithmEnv env)
		{
			var allDestDirs = env.FileSystem.EnumerateDirsRecursive(env.Options.DestinationDirectory);
			Assert.AreEqual(1, allDestDirs.Count());
			Assert.AreEqual(0, env.FileSystem.EnumerateFiles(allDestDirs).Count());
		}
	}
}