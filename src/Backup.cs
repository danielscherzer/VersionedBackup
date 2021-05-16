using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

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
		internal static void Run(ILogger logger, IOptions options, IFileSystem fileSystem, CancellationToken token)
		{
			FileInfo? GetFileInfo(string path)
			{
				try
				{
					return new FileInfo(path);
				}
				catch (SystemException e)
				{
					if (options.LogErrors) logger.Log(e.Message);
					return null;
				}
			}
			void LogOperation(string message)
			{
				if (options.LogOperations) logger.Log(message);
			}

			List<string> report = new();
			void Report(string message)
			{
				if (!options.DryRun) report.Add(message);
			}
			var src = options.SourceDirectory;
			var dst = options.DestinationDirectory;
			var oldFilesFolder = options.OldFilesFolder;

			fileSystem.CreateDirectory(dst);

			var srcDirs = Task.Factory.StartNew(src.EnumerateDirsRecursive()
				.Ignore(options.IgnoreDirectories).ToArray, token);
			var dstDirs = Task.Factory.StartNew(dst.EnumerateDirsRecursive().ToArray, token);

			var srcFilesRelative = Task.Factory.StartNew(() 
				=> srcDirs.Result.EnumerateFiles().ToRelative(src)
				.Ignore(options.IgnoreFiles).ToHashSet(), token);
			var dstFilesRelative = Task.Factory.StartNew(() 
				=> dstDirs.Result.EnumerateFiles().ToRelative(dst).ToHashSet(), token);

			var srcDirsRelative = srcDirs.Result.ToRelative(src).ToHashSet();
			var dstDirsRelative = dstDirs.Result.ToRelative(dst).ToHashSet();

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
				string moveAwaySubDir = dst + subDir;
				if (Directory.Exists(moveAwaySubDir))
				{
					string destination = oldFilesFolder + subDir;
					fileSystem.MoveDirectory(moveAwaySubDir, destination);
					LogOperation($"Moving old directory '{subDir}' to {oldFilesFolder}");
					Report($"Old directory '{subDir}'");
				}
			}
			// find files in dst, but not in src
			var filesToMove = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative));
			foreach (var fileName in filesToMove)
			{
				if (token.IsCancellationRequested) return;
				// move deleted to oldFilesFolder
				string moveAwayFileName = dst + fileName;
				if (File.Exists(moveAwayFileName))
				{
					string destination = oldFilesFolder + fileName;
					fileSystem.MoveFile(moveAwayFileName, destination);
					LogOperation($"Moving deleted file '{fileName}' to '{oldFilesFolder}'");
					Report($"Deleted file '{fileName}'");
				}
			}

			Parallel.ForEach(srcFilesRelative.Result, fileName =>
			{
				if (token.IsCancellationRequested) return;
				var srcFilePath = src + fileName;
				var dstFilePath = dst + fileName;
				if (dstFilesRelative.Result.Contains(fileName))
				{
					var srcFileInfo = GetFileInfo(srcFilePath);
					if (srcFileInfo is null) return;
					var dstFileInfo = GetFileInfo(dstFilePath);
					if (dstFileInfo is null) return;
					TimeSpan writeDiff = srcFileInfo.LastWriteTimeUtc.Subtract(dstFileInfo.LastWriteTimeUtc);
					if (Math.Abs(writeDiff.TotalSeconds) > 5 || srcFileInfo.Length != dstFileInfo.Length)
					{
						// move old to oldFilesFolder
						fileSystem.MoveFile(dstFilePath, oldFilesFolder + fileName);
						LogOperation($"Moving old version of file '{fileName}' to '{oldFilesFolder}'");
						Report($"Old verion of file '{fileName}'");
						// copy new to dst
						fileSystem.Copy(srcFilePath, dstFilePath);
						LogOperation($"Copy new version of file '{fileName}' to '{dst}'");
					}
				}
				else
				{
					// copy new to dst
					fileSystem.Copy(srcFilePath, dstFilePath);
					LogOperation($"Copy new file '{fileName}' to '{dst}'");
				}
			});
			if(0 < report.Count) File.WriteAllLines(oldFilesFolder + "report.txt", report);
		}
	}
}
