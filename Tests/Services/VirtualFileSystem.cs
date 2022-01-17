using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VersionedBackup.Interfaces;
using VersionedBackup.PathHelper;

namespace VersionedBackupTests.Services
{
	internal class VirtualFileSystem : IFileSystem
	{
		public bool Copy(string srcFilePath, string dstFilePath) => files.TryAdd(dstFilePath, 0);

		public bool CreateDirectory(string name) => dirs.TryAdd(NormalizeDir(name), 0);

		public IEnumerable<string> EnumerateDirsRecursive(string dir)
		{
			dir = NormalizeDir(dir);
			return dirs.Keys.Where(subDir => subDir.StartsWith(dir)).Select(subDir => subDir);
		}

		public IEnumerable<string> EnumerateFiles(IEnumerable<string> dirs) =>
			from dir in dirs.AsParallel()
			from file in files.Keys
			where file.StartsWith(dir)
			select file;

		public bool ExistsDirectory(string name) => dirs.ContainsKey(NormalizeDir(name));

		public bool ExistsFile(string name) => files.ContainsKey(name);

		public bool HasChanged(string srcFilePath, string dstFilePath)
		{
			return false;
		}

		public bool MoveDirectory(string source, string destination)
		{
			return dirs.Keys.Remove(source) && dirs.TryAdd(destination, 0);
		}

		public bool MoveFile(string source, string destination)
		{
			return files.Keys.Remove(source) && files.TryAdd(destination, 0);
		}

		internal void CreateFile(string name) => files.TryAdd(name, 0);

		private readonly ConcurrentDictionary<string,int> dirs = new();
		private readonly ConcurrentDictionary<string, int> files = new();

		private static string NormalizeDir(string dir) => dir.IncludeTrailingPathDelimiter();
		//private static string NormalizeDir(string dir) => Path.TrimEndingDirectorySeparator(dir); // will not work because of make relative
	}
}