using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;

namespace VersionedCopy
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
		internal static void Run(IOptions options, IFileSystem fileSystem, CancellationToken token)
		{
			var src = options.SourceDirectory;
			var dst = options.DestinationDirectory;
			var oldFilesFolder = options.OldFilesFolder;

			fileSystem.CreateDirectory(dst);

			var srcDirs = Task.Factory.StartNew(src.EnumerateDirsRecursive()
				.Ignore(options.IgnoreDirectories).ToArray, token);
			var dstDirs = Task.Factory.StartNew(dst.EnumerateDirsRecursive().ToArray, token);

			var srcFilesRelative = Task.Factory.StartNew(() 
				=> srcDirs.Result.EnumerateFiles().ToRelative(src), token);
			var dstFilesRelative = Task.Factory.StartNew(() 
				=> dstDirs.Result.EnumerateFiles().ToRelative(dst), token);

			var srcDirsRelative = srcDirs.Result.ToRelative(src);
			var dstDirsRelative = dstDirs.Result.ToRelative(dst);

			//create missing directories in dst
			var newDirs = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in newDirs)
			{
				if (token.IsCancellationRequested) return;
				fileSystem.CreateDirectory(dst + subDir);
			}

			// find dirs in dst, but not in src
			var dirsToMove = dstDirsRelative.Where(dstDir => !srcDirsRelative.Contains(dstDir));

			// make sure dst enumeration task has ended before changing directories and files
			dstFilesRelative.Wait(token);

			// move away old directories
			foreach (var subDir in dirsToMove)
			{
				if (token.IsCancellationRequested) return;
				if (Directory.Exists(dst + subDir)) fileSystem.MoveDirectory(dst + subDir, oldFilesFolder + subDir);
			}
			// find files in dst, but not in src
			var filesToMove = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative));
			foreach (var fileName in filesToMove)
			{
				if (token.IsCancellationRequested) return;
				// move deleted to oldFilesFolder
				if (File.Exists(dst + fileName)) fileSystem.MoveFile(dst + fileName, oldFilesFolder + fileName);
			}

			CopyParallel(fileSystem, src, dst, oldFilesFolder, srcFilesRelative.Result, dstFilesRelative.Result, token);
		}

		private static void CopyParallel(IFileSystem fileSystem, string src, string dst, string oldFilesFolder, IEnumerable<string> srcFilesRelative, HashSet<string> dstFilesRelative, CancellationToken token)
		{
			Parallel.ForEach(srcFilesRelative, fileName =>
			{
				if (token.IsCancellationRequested) return;
				var srcFilePath = src + fileName;
				var dstFilePath = dst + fileName;
				if (dstFilesRelative.Contains(fileName))
				{
					var srcFileInfo = fileSystem.GetFileInfo(srcFilePath);
					if (srcFileInfo is null) return;
					var dstFileInfo = fileSystem.GetFileInfo(dstFilePath);
					if (dstFileInfo is null) return;
					if (srcFileInfo.LastWriteTimeUtc != dstFileInfo.LastWriteTimeUtc || srcFileInfo.Length != dstFileInfo.Length)
					{
						// move old to oldFilesFolder
						fileSystem.MoveFile(dstFilePath, oldFilesFolder + fileName);
						// copy new to dst
						fileSystem.Copy(srcFilePath, dstFilePath);
					}
				}
				else
				{
					// copy new to dst
					fileSystem.Copy(srcFilePath, dstFilePath);
				}
			});
		}
	}
}
