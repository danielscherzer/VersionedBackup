using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace VersionedCopy
{
	[Serializable]
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

	[Serializable]
	public class DirectoryState
	{
		public DirectoryState(string name)
		{
			Name = name;
		}

		public string Name { get; }
		public HashSet<DirectoryState> Directories { get; } = new();
		public HashSet<FileState> Files { get; } = new();
		public override string ToString() => Name;
	}

	public class StoreState
	{
		public static void Run(string directory, string databaseFileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		{
			var root = new DirectoryInfo(directory);
			if (!root.Exists) return;
			var rootState = new DirectoryState(root.Name);

			var stack = new Stack<(DirectoryInfo, DirectoryState)>();
			stack.Push((root, rootState));
			while (stack.Count > 0)
			{
				(DirectoryInfo dir, DirectoryState dirState) = stack.Pop();
				try
				{
					foreach (var file in dir.EnumerateFiles())
					{
						if (ignoreFiles.Contains(file.Name)) continue;
						dirState.Files.Add(new FileState(file.Name, file.LastWriteTimeUtc));
					}
					foreach (var subDir in dir.EnumerateDirectories())
					{
						if (ignoreDirectories.Contains(subDir.Name + Path.DirectorySeparatorChar)) continue;
						var subDirState = new DirectoryState(subDir.Name);
						dirState.Directories.Add(subDirState);
						stack.Push((subDir, subDirState));
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
			string json = JsonConvert.SerializeObject(rootState, Formatting.Indented);
			File.WriteAllText(databaseFileName, json);
		}
	}
}
