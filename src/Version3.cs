using System.Linq;

namespace VersionedCopy
{
	class Version3
	{
		internal static void Run(Benchmark benchmark, string src, string dst, string oldFilesFolder)
		{
			var srcDirs = src.EnumerateDirsRecursive().ToArray();
			var dstDirs = dst.EnumerateDirsRecursive().ToArray();

			var srcFilesRelative = srcDirs.EnumerateFiles().Select(file => file[src.Length..]).ToHashSet();
			var dstFilesRelative = dstDirs.EnumerateFiles().Select(file => file[dst.Length..]).ToHashSet();

			var filesToDelete = dstFilesRelative.Where(dstFileRelative => !srcFilesRelative.Contains(dstFileRelative)); // only in dst
		}
	}
}