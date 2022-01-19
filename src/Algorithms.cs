using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
			if (!fileSystem.ExistsDirectory(src))
			{
				report.Error($"Source directory '{src}' does not exist");
				return;
			}

			FileSystemOperations op = new(report, options, fileSystem);

			if (!fileSystem.ExistsDirectory(dst)) op.CreateDirectory("");

			var srcDirs = Task.Run(fileSystem.EnumerateDirsRecursive(src)
				.Ignore(options.IgnoreDirectories).ToArray, token);
			var dstDirs = Task.Run(fileSystem.EnumerateDirsRecursive(dst)
				.Ignore(options.IgnoreDirectories).ToArray, token);

			var srcDirsRelative = srcDirs.Result.ToRelative(src).ToHashSet();
			var dstDirsRelative = dstDirs.Result.ToRelative(dst).ToHashSet();

			var srcFilesRelative = Task.Run(()
				=> fileSystem.EnumerateFiles(srcDirs.Result).ToRelative(src)
				.Ignore(options.IgnoreFiles).ToHashSet(), token);
			var dstFilesRelative = Task.Run(()
				=> fileSystem.EnumerateFiles(dstDirs.Result).ToRelative(dst)
				.Ignore(options.IgnoreFiles).ToHashSet(), token);

			switch (options.Mode)
			{
				case AlgoMode.Mirror:
					Mirror(srcDirsRelative, dstDirsRelative, srcFilesRelative, dstFilesRelative, op, token);
					break;
				case AlgoMode.Sync:
					Sync(srcDirsRelative, dstDirsRelative, srcFilesRelative, dstFilesRelative, op, token);
					break;
				case AlgoMode.Update:
					Update(srcDirsRelative, dstDirsRelative, srcFilesRelative, dstFilesRelative, op, token);
					break;
			}
		}

		private static void Mirror(HashSet<string> srcDirsRelative, HashSet<string> dstDirsRelative, Task<HashSet<string>> srcFilesRelative, Task<HashSet<string>> dstFilesRelative, FileSystemOperations op, CancellationToken token)
		{
			//create missing directories in dst
			var newDirs = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in newDirs)
			{
				if (token.IsCancellationRequested) return;
				op.CreateDirectory(subDir);
			}

			// make sure dst enumeration task has ended before changing directories and files
			dstFilesRelative.Wait(token);

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
				op.MoveAwayDeletedFile(fileName);
			}

			try
			{
				Parallel.ForEach(srcFilesRelative.Result, new ParallelOptions { CancellationToken = token }, fileName =>
				{
					if (dstFilesRelative.Result.Contains(fileName))
					{
						op.ReplaceChangedFile(fileName);
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

		private static void Sync(HashSet<string> srcDirsRelative, HashSet<string> dstDirsRelative, Task<HashSet<string>> srcFilesRelative, Task<HashSet<string>> dstFilesRelative, FileSystemOperations op, CancellationToken token)
		{
			//create missing directories in dst
			var srcDirSingles = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in srcDirSingles)
			{
				if (token.IsCancellationRequested) return;
				//TODO: if in delete log move to old dir
				op.CreateDirectory(subDir);
			}

			// make sure dst enumeration task has ended before changing directories and files
			dstFilesRelative.Wait(token);

			// find directories in dst, but not in src
			var dstDirSingles = dstDirsRelative.Where(dstDir => !srcDirsRelative.Contains(dstDir));
			// create missing directories in src
			foreach (var subDir in dstDirSingles)
			{
				if (token.IsCancellationRequested) return;
				//TODO: if in delete log move to old dir
				op.CreateSrcDirectory(subDir);
			}

			// find files in src, but not in dst
			var srcFileSingles = srcFilesRelative.Result.Where(srcFileRelative => !dstFilesRelative.Result.Contains(srcFileRelative));
			foreach (var fileName in srcFileSingles)
			{
				if (token.IsCancellationRequested) return;
				op.CopyNewFile(fileName);
				//TODO: if in delete log op.MoveAwayDeleted(fileName);
			}

			// find files in dst, but not in src
			var dstFileSingles = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative));
			foreach (var fileName in dstFileSingles)
			{
				if (token.IsCancellationRequested) return;
				op.CopyNewFileToSrc(fileName);
				//TODO: if in delete log op.MoveAwayDeleted(fileName);
			}

			// in both
			srcFilesRelative.Result.IntersectWith(dstFilesRelative.Result);
			try
			{
				Parallel.ForEach(srcFilesRelative.Result, new ParallelOptions { CancellationToken = token }, fileName =>
				{
					op.CopyNewerFileToOtherSide(fileName);
				});
			}
			catch (OperationCanceledException)
			{
			}
		}

		private static void Update(HashSet<string> srcDirsRelative, HashSet<string> dstDirsRelative, Task<HashSet<string>> srcFilesRelative, Task<HashSet<string>> dstFilesRelative, FileSystemOperations op, CancellationToken token)
		{
			//create missing directories in dst
			var newDirs = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in newDirs)
			{
				if (token.IsCancellationRequested) return;
				op.CreateDirectory(subDir);
			}

			// make sure dst enumeration task has ended before changing files
			dstFilesRelative.Wait(token);

			try
			{
				Parallel.ForEach(srcFilesRelative.Result, new ParallelOptions { CancellationToken = token }, fileName =>
				{
					if (dstFilesRelative.Result.Contains(fileName))
					{
						op.CopyUpdatedFile(fileName);
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
