using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VersionedCopy
{
	class Version3MT
	{
		internal static void Run(Benchmark benchmark, string src, string dst, string oldFilesFolder)
		{
			benchmark.Reset(nameof(Version3MT));
			// optimize common update case: assume two directory structures are very similar
			// 1=> read multi-threaded all directories and then all files
			// 2=> compare almost all files for date time and size

			var srcDirs = Task.Factory.StartNew(src.EnumerateDirsRecursive().ToArray);
			var dstDirs = Task.Factory.StartNew(dst.EnumerateDirsRecursive().ToArray);

			var srcFilesRelative = Task.Factory.StartNew(srcDirs.Result.EnumerateFiles().Select(file => file[src.Length..]).ToHashSet);
			var dstFilesRelative = Task.Factory.StartNew(dstDirs.Result.EnumerateFiles().Select(file => file[dst.Length..]).ToHashSet);
			//benchmark.Delta("dirs");

			var filesToDelete = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative)); // only in dst
																																	  //benchmark.Delta("files");

			//benchmark.Delta("move files");
			//benchmark.Delta("del dirs");
			//benchmark.Delta("check and copy files");
			benchmark.Total("-------------------------------");
		}
	}
}