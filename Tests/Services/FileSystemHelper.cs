using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace VersionedCopy.Tests.Services
{
	internal static class FileSystemHelper
	{
		public static readonly string Root = Path.Combine(Path.GetTempPath(), "VersionedCopy");

		public static void CreateFile(string fileName) => File.WriteAllText(fileName, fileName);
		public static void FillDirTree(string rootName, int size)
		{
			var root = new DirectoryInfo(rootName);
			root.Create();
			CreateFile(Path.Combine(root.FullName, "file"));
			for (int i = 0; i < size; ++i)
			{
				var subDir = root.CreateSubdirectory(i.ToString());
				if (size / 2 > i)
				{
					for (int j = 0; j < size; ++j)
					{
						CreateFile(Path.Combine(subDir.FullName, $"file{j}"));
					}
				}
			}
		}

		public static string Read(params string[] nameParts)
		{
			var path = Path.Combine(nameParts);
			return File.ReadAllText(path);
		}

		public static string Create(params string[] nameParts)
		{
			var path = Path.Combine(nameParts);
			if (Path.EndsInDirectorySeparator(path))
			{
				Directory.CreateDirectory(path);
				return "";
			}
			else
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllText(path, path);
				return path;
			}
		}

		public static void Exists(params string[] nameParts)
		{
			var path = Path.Combine(nameParts);
			if (Path.EndsInDirectorySeparator(path))
			{
				Assert.IsTrue(Directory.Exists(path));
			}
			else
			{
				Assert.IsTrue(File.Exists(path));
			}
		}

		internal static void RndFill(this VirtualFileSystem fileSystem, string root, int seed, string dirPrefix = "", Action<string> addDir = null, Action<string> addFile = null)
		{
			Random rnd = new(seed);

			void Fill(string dir)
			{
				// subdirs
				for (int i = rnd.Next(0, 10); i > 0; --i)
				{
					char c = (char)('A' + i);
					string path = Path.Combine(dir, dirPrefix + c);
					fileSystem.CreateDirectory(path);
					addDir?.Invoke(path);
					if (0 == rnd.Next(10)) Fill(path); // recursion
				}
				// files
				for (int i = rnd.Next(0, 30); i > 0; --i)
				{
					string path = Path.Combine(dir, i.ToString());
					fileSystem.CreateFile(path);
					addFile?.Invoke(path);
				}
			}
			fileSystem.CreateDirectory(root);
			Fill(root);
		}
	}
}
