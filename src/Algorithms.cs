using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public static class Algorithms
	{
		/// <summary>
		/// optimize common update case: assume two directory structures are very similar
		/// 1=> read multi-threaded all directories and then all files
		/// 2=> optimize compare of almost all files for date time and size
		/// </summary>
		/// <param name="options"></param>
		/// <param name="token"><see cref="CancellationToken"/></param>
		public static void Run(IOptions options, IReport report, IFileSystem fileSystem, CancellationToken token)
		{
			var src = options.SourceDirectory.IncludeTrailingPathDelimiter();
			var dst = options.DestinationDirectory.IncludeTrailingPathDelimiter();
			FileSystemOperations op = new(report, options, fileSystem);
			if (!fileSystem.ExistsDirectory(src))
			{
				report.Error($"Source directory '{src}' does not exist");
				return;
			}

			if (!fileSystem.ExistsDirectory(dst)) op.CreateDirectory("");

			var srcDirs = Task.Run(fileSystem.EnumerateDirsRecursive(src)
				.Ignore(options.IgnoreDirectories).ToArray, token);
			var dstDirs = Task.Run(fileSystem.EnumerateDirsRecursive(dst).ToArray, token);

			var srcFilesRelative = Task.Run(()
				=> fileSystem.EnumerateFiles(srcDirs.Result).ToRelative(src)
				.Ignore(options.IgnoreFiles).ToHashSet(), token);
			var dstFilesRelative = Task.Run(()
				=> fileSystem.EnumerateFiles(dstDirs.Result).ToRelative(dst).ToHashSet(), token);

			var srcDirsRelative = srcDirs.Result.ToRelative(src).ToHashSet();
			var dstDirsRelative = dstDirs.Result.ToRelative(dst).ToHashSet();

			//create missing directories in dst
			var newDirs = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in newDirs)
			{
				if (token.IsCancellationRequested) return;
				op.CreateDirectory(subDir);
			}

			// make sure dst enumeration task has ended before changing directories and files
			dstFilesRelative.Wait(token);

			if(options.Mode != AlgoMode.Update)
			{
				// find directories in dst, but not in src
				var dirsToMove = dstDirsRelative.Where(dstDir => !srcDirsRelative.Contains(dstDir));
				// move away old directories
				foreach (var subDir in dirsToMove)
				{
					if (token.IsCancellationRequested) return;
					op.MoveAwayDeletedDir(subDir);
				}
				// find files in dst, but not in src
				var filesToMove = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative));
				foreach (var fileName in filesToMove)
				{
					if (token.IsCancellationRequested) return;
					op.MoveAwayDeleted(fileName);
				}
			}

			try
			{
				bool mirror = options.Mode == AlgoMode.Mirror;
				Parallel.ForEach(srcFilesRelative.Result, new ParallelOptions { CancellationToken = token }, fileName =>
				{
					if (dstFilesRelative.Result.Contains(fileName))
					{
						if(mirror) op.CopyChangedFile(fileName);
						else op.CopyUpdatedFile(fileName);
					}
					else
					{
						op.CopyNewFile(fileName);
					}
				});
			}
			catch (OperationCanceledException)
			{
			}
		}
	}
}
