using System.Collections.Generic;
using System.Linq;
using VersionedBackup.Interfaces;
using VersionedBackup.PathHelper;

namespace VersionedBackupTests.Services
{
	internal class VirtualFileSystem : IFileSystem
	{
		public bool Copy(string srcFilePath, string dstFilePath) => files.Add(dstFilePath);

		public bool CreateDirectory(string name) => dirs.Add(NormalizeDir(name));

		public IEnumerable<string> EnumerateDirsRecursive(string dir)
		{
			dir = NormalizeDir(dir);
			return dirs.Where(subDir => subDir.StartsWith(dir)).Select(subDir => subDir);
		}

		public IEnumerable<string> EnumerateFiles(IEnumerable<string> dirs) =>
			from dir in dirs.AsParallel()
			from file in files
			where file.StartsWith(dir)
			select file;

		public bool ExistsDirectory(string name) => dirs.Contains(NormalizeDir(name));

		public bool ExistsFile(string name) => files.Contains(name);

		public bool HasChanged(string srcFilePath, string dstFilePath)
		{
			return false;
		}

		public bool MoveDirectory(string source, string destination)
		{
			return dirs.Remove(source) && dirs.Add(destination);
		}

		public bool MoveFile(string source, string destination)
		{
			return files.Remove(source) && files.Add(destination);
		}

		internal void CreateFile(string name) => files.Add(name);

		private readonly HashSet<string> dirs = new();
		private readonly HashSet<string> files = new();

		private static string NormalizeDir(string dir) => dir.IncludeTrailingPathDelimiter();
		//private static string NormalizeDir(string dir) => Path.TrimEndingDirectorySeparator(dir); // will not work because of make relative
	}
}