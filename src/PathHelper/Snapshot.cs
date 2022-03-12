using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VersionedCopy.PathHelper
{
	public class Snapshot
	{
		public Snapshot(string root)
		{
			Root = root.IncludeTrailingPathDelimiter();
		}

		public void Add(string name, DateTime writeTime) => Entries.Add(name, writeTime);

		public SortedDictionary<string, DateTime> Entries { get; } = new();

		public string FullName(string fileName) => Root + fileName;
		
		public string Root { get; private set; }

		public static bool IsFile(string fileName) => !Path.EndsInDirectorySeparator(fileName);

		public RelativeFileList Files() => new(Root, Entries.Where(entry => IsFile(entry.Key)));

		public static Snapshot Create(string directory, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, System.Threading.CancellationToken cancellationToken)
		{
			Snapshot snapshot = new(directory);
			var root = new DirectoryInfo(snapshot.Root);
			if (!root.Exists) throw new DirectoryNotFoundException(directory);
			var regexIgnoreDirectories = ignoreDirectories.CreateIgnoreRegex().ToList();
			var regexIgnoreFiles = ignoreFiles.CreateIgnoreRegex().ToList();

			string ToRelative(string fullName) => fullName[root.FullName.Length..];

			Queue<DirectoryInfo> subDirs = new();
			subDirs.Enqueue(root);
			while (subDirs.Count > 0)
			{
				DirectoryInfo dir = subDirs.Dequeue();
				try
				{
					foreach (var file in dir.EnumerateFiles())
					{
						if (cancellationToken.IsCancellationRequested) return snapshot;
						var relativName = ToRelative(file.FullName);
						if (regexIgnoreFiles.AnyMatch(Path.DirectorySeparatorChar + relativName)) continue;
						snapshot.Add(relativName, file.LastWriteTimeUtc);
					}
					foreach (var subDir in dir.EnumerateDirectories())
					{
						if (cancellationToken.IsCancellationRequested) return snapshot;
						var relativName = ToRelative(subDir.FullName) + Path.DirectorySeparatorChar;
						if (regexIgnoreDirectories.AnyMatch(Path.DirectorySeparatorChar + relativName)) continue;
						snapshot.Add(relativName, subDir.CreationTimeUtc);
						subDirs.Enqueue(subDir);
					}
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex.Message);
				}
			}
			return snapshot;
		}

		public RelativeFileList Singles(Snapshot other) => new(Root, Entries.Where(dir => !other.Entries.ContainsKey(dir.Key)));

		public static void Run(string directory, string databaseFileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, System.Threading.CancellationToken cancellationToken)
		{
			Console.WriteLine($"Store state '{directory}' to '{databaseFileName}'");
			var state = Create(directory, ignoreDirectories, ignoreFiles, cancellationToken);
			string json = JsonConvert.SerializeObject(state, Formatting.Indented);
			File.WriteAllText(databaseFileName, json);
		}
	}
}
