using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VersionedCopy
{
	internal class Version3MT
	{
		internal static void Run(Benchmark benchmark, string src, string dst, string oldFilesFolder)
		{
			// optimize common update case: assume two directory structures are very similar
			// 1=> read multi-threaded all directories and then all files
			// 2=> compare almost all files for date time and size

			var srcDirs = Task.Factory.StartNew(src.EnumerateDirsRecursive().ToArray);
			var dstDirs = Task.Factory.StartNew(dst.EnumerateDirsRecursive().ToArray);

			var srcFilesRelative = Task.Factory.StartNew(srcDirs.Result.EnumerateFiles().Select(file => file[src.Length..]).ToHashSet);
			var dstFilesRelative = Task.Factory.StartNew(dstDirs.Result.EnumerateFiles().Select(file => file[dst.Length..]).ToHashSet);

			// find files in dst, but not in src
			var filesToMove = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative));
			foreach (var fileName in filesToMove)
			{
				// move deleted to oldFilesFolder
				FileSystem.Move(dst + fileName, oldFilesFolder + fileName);
			}

			//Parallel.ForEach(srcFilesRelative.Result, fileName =>
			foreach (var fileName in srcFilesRelative.Result)
			{
				var srcFilePath = src + fileName;
				var dstFilePath = dst + fileName;
				if (dstFilesRelative.Result.Contains(fileName))
				{
					//var srcFileInfo = new FileInfo(srcFilePath);
					//var dstFileInfo = new FileInfo(dstFilePath);
					//if (srcFileInfo.Length != dstFileInfo.Length || srcFileInfo.LastWriteTimeUtc != dstFileInfo.LastWriteTimeUtc)
					//{
					//	// move old to oldFilesFolder
					//	FileSystem.Move(dstFilePath, oldFilesFolder + fileName);
					//	// copy new to dst
					//	File.Copy(srcFilePath, dstFilePath);
					//	//Console.WriteLine($"Updated '{fileName}'");
					//}
				}
				else
				{
					// copy new to dst
					File.Copy(srcFilePath, dstFilePath);
					//Console.WriteLine($"Copied '{fileName}'");
				}
			}//);
		}
	}
}