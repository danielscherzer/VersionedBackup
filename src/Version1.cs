using System;
using System.IO;
using System.Linq;

namespace VersionedCopy
{
	class Version1
	{
		internal static void Run(Benchmark benchmark, string src, string dst, string oldFilesFolder)
		{
			var srcDirs = src.EnumerateDirsRecursive().ToArray();

			//create directory structure in dst
			Directory.CreateDirectory(dst);
			foreach (var dir in srcDirs)
			{
				var subDir = dst + dir[src.Length..]; //replace src with dst
				Directory.CreateDirectory(subDir);
			}

			var dstDirs = dst.EnumerateDirsRecursive().ToArray();

			var srcFilesRelative = srcDirs.EnumerateFiles().Select(file => file[src.Length..]).ToHashSet();
			var dstFilesRelative = dstDirs.EnumerateFiles().Select(file => file[dst.Length..]).ToHashSet();

			var filesToDelete = dstFilesRelative.Where(dstFileRelative => !srcFilesRelative.Contains(dstFileRelative)); // only in dst

			foreach (var fileName in filesToDelete)
			{
				// move deleted to date folder
				FileSystem.Move(dst + fileName, oldFilesFolder + fileName);
			}

			//Delete vacant directories
			foreach (var dstDir in dstDirs.Reverse())
			{
				var srcDir = src + dstDir[dst.Length..]; //replace dst with src
				if (!Directory.Exists(srcDir))
				{
					Directory.Delete(dstDir);
				}
			}

			foreach (var fileName in srcFilesRelative)
			{
				var srcFilePath = src + fileName;
				var dstFilePath = dst + fileName;
				if (dstFilesRelative.Contains(fileName))
				{
					var srcFileInfo = new FileInfo(srcFilePath);
					var dstFileInfo = new FileInfo(dstFilePath);
					if (srcFileInfo.Length != dstFileInfo.Length || srcFileInfo.LastWriteTimeUtc != dstFileInfo.LastWriteTimeUtc)
					{
						// move old to oldFilesFolder
						FileSystem.Move(dstFilePath, oldFilesFolder + fileName);
						// copy new to dst
						File.Copy(srcFilePath, dstFilePath);
						//Console.WriteLine($"Updated '{fileName}'");
					}
				}
				else
				{
					// copy new to dst
					File.Copy(srcFilePath, dstFilePath);
					//Console.WriteLine($"Copied '{fileName}'");
				}
			}
		}
	}
}
