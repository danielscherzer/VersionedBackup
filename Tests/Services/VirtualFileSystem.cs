using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopyTests.Services
{
	public class VirtualFileSystem : IFileSystem
	{
		public int CompareAge(string source, string destination)
		{
			return files[source].CompareTo(files[destination]);
		}

		public bool Copy(string srcFilePath, string dstFilePath) => files.TryAdd(dstFilePath, files[srcFilePath]);

		public bool CreateDirectory(string name) => dirs.TryAdd(NormalizeDir(name), 0);

		public IEnumerable<string> EnumerateDirsRecursive(string dir)
		{
			dir = NormalizeDir(dir);
			return dirs.Keys.Where(subDir => subDir.StartsWith(dir)).ToArray(); // make copy so we can change it when iterating
		}

		public IEnumerable<string> EnumerateFiles(IEnumerable<string> dirs) =>
			(from dir in dirs.AsParallel()
			 from file in files.Keys
			 where Path.GetDirectoryName(file) + Path.DirectorySeparatorChar == dir
			 select file).ToArray(); // make copy so we can change it when iterating

		public bool ExistsDirectory(string name) => dirs.ContainsKey(NormalizeDir(name));

		public bool ExistsFile(string name) => files.ContainsKey(name);

		public bool HasChanged(string srcFilePath, string dstFilePath)
		{
			return files[srcFilePath] != files[dstFilePath];
		}

		public bool MoveDirectory(string source, string destination)
		{
			return DeleteDir(source) && dirs.TryAdd(NormalizeDir(destination), 0);
		}

		public bool MoveFile(string source, string destination)
		{
			return files.Remove(source, out var date) && files.TryAdd(destination, date);
		}

		internal void CreateFile(string name) => files.TryAdd(name, 0);

		internal bool DeleteDir(string name) => dirs.Remove(NormalizeDir(name), out _);
		internal bool DeleteFile(string name) => files.Remove(name, out _);
		internal void UpdateFile(string name) => ++files[name];

		private readonly ConcurrentDictionary<string, int> dirs = new();
		private readonly ConcurrentDictionary<string, int> files = new();

		private static string NormalizeDir(string dir) => dir.IncludeTrailingPathDelimiter();
		//private static string NormalizeDir(string dir) => Path.TrimEndingDirectorySeparator(dir); // will not work because of make relative
	}
}