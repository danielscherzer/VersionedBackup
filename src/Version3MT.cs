using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VersionedCopy
{
	internal class Version3MT
	{
		/// <summary>
		/// optimize common update case: assume two directory structures are very similar
		/// 1=> read multi-threaded all directories and then all files
		/// 2=> optimize compare of almost all files for date time and size
		/// </summary>
		/// <param name="options"></param>
		internal static void Run(Options options, CancellationToken token)
		{
			var src = options.SourceDirectory;
			var dst = options.DestinationDirectory;
			var oldFilesFolder = options.OldFilesFolder;

			var srcDirs = Task.Factory.StartNew(src.EnumerateDirsRecursive().ToArray, token);
			var dstDirs = Task.Factory.StartNew(dst.EnumerateDirsRecursive().ToArray, token);

			var srcFilesRelative = Task.Factory.StartNew(srcDirs.Result.EnumerateFiles().Select(file => file[src.Length..]).ToHashSet, token);
			var dstFilesRelative = Task.Factory.StartNew(dstDirs.Result.EnumerateFiles().Select(file => file[dst.Length..]).ToHashSet, token);

			var srcDirsRelative = srcDirs.Result.Select(srcDir => srcDir[src.Length..]).ToHashSet();
			var dstDirsRelative = dstDirs.Result.Select(dstDir => dstDir[dst.Length..]).ToHashSet();

			//create missing directories in dst
			var newDirs = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in newDirs)
			{
				//if (token.IsCancellationRequested) return;
				FileSystem.CreateDirectory(dst + subDir);
			}

			// find dirs in dst, but not in src
			var dirsToMove = dstDirsRelative.Where(dstDir => !srcDirsRelative.Contains(dstDir));

			// make sure enumeration task have ended before changing directories and files
			Task.WaitAll(new[] { srcFilesRelative, dstFilesRelative }, token);

			// move away old directories
			foreach (var subDir in dirsToMove)
			{
				//if (token.IsCancellationRequested) return;
				if (Directory.Exists(dst + subDir)) FileSystem.Move(dst + subDir, oldFilesFolder + subDir);
			}
			// find files in dst, but not in src
			var filesToMove = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative));
			foreach (var fileName in filesToMove)
			{
				//if (token.IsCancellationRequested) return;
				// move deleted to oldFilesFolder
				if (File.Exists(dst + fileName)) FileSystem.Move(dst + fileName, oldFilesFolder + fileName);
			}

			CopyParallel(src, dst, oldFilesFolder, srcFilesRelative.Result, dstFilesRelative.Result, token);
		}

		private static void CopyParallel(string src, string dst, string oldFilesFolder, IEnumerable<string> srcFilesRelative, HashSet<string> dstFilesRelative, CancellationToken token)
		{
			Parallel.ForEach(srcFilesRelative, fileName =>
			{
				//if (token.IsCancellationRequested) return;
				var srcFilePath = src + fileName;
				var dstFilePath = dst + fileName;
				if (dstFilesRelative.Contains(fileName))
				{
					var srcFileInfo = FileSystem.GetFileInfo(srcFilePath);
					if (srcFileInfo is null) return;
					var dstFileInfo = FileSystem.GetFileInfo(dstFilePath);
					if (dstFileInfo is null) return;
					if (srcFileInfo.LastWriteTimeUtc != dstFileInfo.LastWriteTimeUtc || srcFileInfo.Length != dstFileInfo.Length)
					{
						// move old to oldFilesFolder
						FileSystem.Move(dstFilePath, oldFilesFolder + fileName);
						// copy new to dst
						FileSystem.Copy(srcFilePath, dstFilePath);
						//Console.WriteLine($"Updated '{fileName}'");
					}
				}
				else
				{
					// copy new to dst
					FileSystem.Copy(srcFilePath, dstFilePath);
					//Console.WriteLine($"Copied '{fileName}'");
				}
			});
		}
	}
}