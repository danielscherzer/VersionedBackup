using System;
using System.IO;

namespace VersionedCopyTests.Services
{
	internal static class FillFileSystem
	{
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
