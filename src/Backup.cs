using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VersionedBackup.Interfaces;
using VersionedBackup.PathHelper;
using VersionedBackup.Services;

namespace VersionedBackup
{
	internal static class Backup
	{
		/// <summary>
		/// optimize common update case: assume two directory structures are very similar
		/// 1=> read multi-threaded all directories and then all files
		/// 2=> optimize compare of almost all files for date time and size
		/// </summary>
		/// <param name="options"></param>
		/// <param name="token"><see cref="CancellationToken"/></param>
		internal static void Run(IOptions options, FileOperation op, CancellationToken token)
		{
			var src = options.SourceDirectory;
			var dst = options.DestinationDirectory;

			op.CreateDirectory(""); // create destination

			var srcDirs = Task.Run(src.EnumerateDirsRecursive()
				.Ignore(options.IgnoreDirectories).ToArray, token);
			var dstDirs = Task.Run(dst.EnumerateDirsRecursive().ToArray, token);

			var srcFilesRelative = Task.Run(()
				=> srcDirs.Result.EnumerateFiles().ToRelative(src)
				.Ignore(options.IgnoreFiles).ToHashSet(), token);
			var dstFilesRelative = Task.Run(()
				=> dstDirs.Result.EnumerateFiles().ToRelative(dst).ToHashSet(), token);

			var srcDirsRelative = srcDirs.Result.ToRelative(src).ToHashSet();
			var dstDirsRelative = dstDirs.Result.ToRelative(dst).ToHashSet();

			//create missing directories in dst
			var newDirs = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in newDirs)
			{
				if (token.IsCancellationRequested) return;
				op.CreateDirectory(subDir);
			}

			// find directories in dst, but not in src
			var dirsToMove = dstDirsRelative.Where(dstDir => !srcDirsRelative.Contains(dstDir));

			// make sure dst enumeration task has ended before changing directories and files
			dstFilesRelative.Wait(token);

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

			try
			{
				Parallel.ForEach(srcFilesRelative.Result, new ParallelOptions { CancellationToken = token }, fileName =>
				{
					if (dstFilesRelative.Result.Contains(fileName))
					{
						op.UpdateFile(fileName);
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
