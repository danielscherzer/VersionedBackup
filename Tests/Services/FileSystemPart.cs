using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Tests.Services
{
	class FileSystemPart
	{
		public FileSystemPart(VirtualFileSystem fileSystem, string root, int seed, string dirPrefix = "")
		{
			root = root.IncludeTrailingPathDelimiter();
			int relativeStart = root.Length;
			fileSystem.RndFill(root, seed, dirPrefix, path => subDirs.Add(path[relativeStart..]), path => files.Add(path[relativeStart..]));
			this.fileSystem = fileSystem;
		}

		public static void AssertContains(VirtualFileSystem fileSystem, string root, IEnumerable<string> srcFiles, IEnumerable<string> srcSubDirs)
		{
			foreach (var file in srcFiles) Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(root, file)));
			foreach (var subDir in srcSubDirs) Assert.IsTrue(fileSystem.ExistsDirectory(Path.Combine(root, subDir)));
		}

		public IEnumerable<string> Files => files;
		public IEnumerable<string> SubDirs => subDirs;

		internal void AssertContainsPart(string root)
		{
			AssertContains(fileSystem, root, files, subDirs);
		}

		private readonly List<string> files = new();
		private readonly List<string> subDirs = new();
		private readonly VirtualFileSystem fileSystem;
	}
}
