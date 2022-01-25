using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public class FileState
	{
		public FileState(string name, DateTime lastWriteTime)
		{
			Name = name;
			LastWriteTime = lastWriteTime;
		}

		public string Name { get; }
		public DateTime LastWriteTime { get; }
		public override string ToString() => Name;
	}

	public class DirectoryState
	{
		public DirectoryState(string name)
		{
			Name = name;
		}

		public string Name { get; }
		public List<DirectoryState> Directories { get; } = new();
		public List<FileState> Files { get; } = new();
		public override string ToString() => Name;
	}

	public class StoreState
	{
		public static void Run(string directory, string databaseFileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		{
			var root = new DirectoryInfo(directory);
			if (!root.Exists) return;
			var rootState = new DirectoryState(root.Name);

			var subDirs = new Stack<(DirectoryInfo, DirectoryState)>();
			subDirs.Push((root, rootState));
			while (subDirs.Count > 0)
			{
				(DirectoryInfo dir, DirectoryState dirState) = subDirs.Pop();
				try
				{
					foreach (var file in dir.EnumerateFiles())
					{
						//if (ignoreFiles.Contains(file.Name)) continue;
						dirState.Files.Add(new FileState(file.Name, file.LastWriteTimeUtc));
					}
					foreach (var subDir in dir.EnumerateDirectories())
					{
						//if (ignoreDirectories.Contains(subDir.Name + Path.DirectorySeparatorChar)) continue;
						var subDirState = new DirectoryState(subDir.Name);
						dirState.Directories.Add(subDirState);
						subDirs.Push((subDir, subDirState));
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
			//string json = JsonConvert.SerializeObject(rootState, Formatting.Indented);
			//File.WriteAllText(databaseFileName, json);
		}

		public static void Run2(string directory, string databaseFileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		{
			directory = directory.IncludeTrailingPathDelimiter();
			Stopwatch stopwatch = Stopwatch.StartNew();
			HashSet<string> dirs = Directory.GetDirectories(directory, "*.*", SearchOption.AllDirectories)
				.ToRelative(directory).Ignore(ignoreDirectories).ToHashSet();
			stopwatch.Benchmark("dirs");
			var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Ignore(ignoreFiles);
			stopwatch.Benchmark("files");
			Dictionary<string, DateTime> fileInfo = files.ToDictionary(file => file[directory.Length..], file => new FileInfo(file).LastWriteTimeUtc);
			stopwatch.Benchmark("toDic");
			//string json = JsonConvert.SerializeObject(state, Formatting.Indented);
			//File.WriteAllText(databaseFileName, json);
		}
	}
}
