using System.IO;

namespace VersionedCopy
{
	class Version2
	{
		internal static void Run(Benchmark benchmark, string src, string dst, string oldFilesFolder)
		{
			// walk through all folders and files in dst recursively and check if they exist in src
			RemoveLeftovers(dst);
			benchmark.Delta("remove leftovers");

			void RemoveLeftovers(string dstDir)
			{
				var relativeDir = dstDir[dst.Length..];
				if (Directory.Exists(src + relativeDir))
				{
					// directory exists in source -> now check each file and recurse on each sub dir
					foreach (var dstFile in Directory.EnumerateFiles(dstDir))
					{
						var relativeFile = dstFile[dst.Length..];
						if (!File.Exists(src + relativeFile))
						{
							// file does not exist in src -> move it from dst to oldFiles
							FileSystem.Move(dstFile, oldFilesFolder + relativeFile);
						}
					}
					foreach (var subDir in Directory.EnumerateDirectories(dstDir))
					{
						RemoveLeftovers(subDir);
					}
				}
				else
				{
					// directory does not exist in src -> move it from dst to oldFiles
					FileSystem.Move(dstDir, oldFilesFolder + relativeDir);
				}
			}

			// copy all folders and files from src to dst recursively
			Copy(src);
			benchmark.Delta("copy");

			void Copy(string srcDir)
			{
				var relativeDir = srcDir[src.Length..];
				Directory.CreateDirectory(dst + relativeDir);
				//copy all files that changed
				foreach (var srcFile in Directory.EnumerateFiles(srcDir))
				{
					var relativeFile = srcFile[src.Length..];
					var dstFile = dst + relativeFile;
					var dstFileInfo = new FileInfo(dstFile);
					if (dstFileInfo.Exists)
					{
						var srcFileInfo = new FileInfo(srcFile);
						if (srcFileInfo.LastWriteTimeUtc != dstFileInfo.LastWriteTimeUtc || srcFileInfo.Length != dstFileInfo.Length)
						{
							// move old to date folder
							FileSystem.Move(dstFile, oldFilesFolder + relativeFile);
							// copy new to dst
							File.Copy(srcFile, dstFile);
							//Console.WriteLine($"Updated '{fileName}'");
						}
					}
					else
					{
						// copy new to dst
						File.Copy(srcFile, dstFile);
						//Console.WriteLine($"Copied '{fileName}'");
					}
				}
				//recurse on sub dirs
				foreach (var srcSubDir in Directory.EnumerateDirectories(srcDir))
				{
					Copy(srcSubDir);
				}
			}
		}
	}
}
