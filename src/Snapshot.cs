using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VersionedCopy.PathHelper;

namespace VersionedCopy
{
	public class Snapshot
	{
		public void Add(string name, DateTime writeTime) => Entries.Add(name, writeTime);

		public Dictionary<string, DateTime> Entries { get; } = new();

		public static bool IsFile(string fileName) => !Path.EndsInDirectorySeparator(fileName);

		public IEnumerable<KeyValuePair<string, DateTime>> Files() => Entries.Where(entry => IsFile(entry.Key));

		//public static Snapshot Create(string directory, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		//{
		//	var root = directory.IncludeTrailingPathDelimiter();
		//	var dirs = Directory.GetDirectories(directory, "*.*", SearchOption.AllDirectories)
		//		.ToRelative(root)
		//		.Select(dir => dir.IncludeTrailingPathDelimiter())
		//		.Ignore(ignoreDirectories);

		//	var dirHash = dirs.ToHashSet();
		//	var regexIgnoreFiles = ignoreFiles.CreateIgnoreRegex().ToList();

		//	var files = from subDir in dirHash.Prepend("")
		//				from file in Directory.EnumerateFiles(root + subDir)
		//				where !regexIgnoreFiles.AnyMatch(file)
		//				select file;

		//	var fileInfo = files.ToDictionary(file => file[root.Length..],
		//		file => File.GetLastWriteTimeUtc(file)
		//		//file => DateTime.Now
		//		);
		//	return new Snapshot(dirHash, fileInfo, DateTime.Now);
		//}

		public static Snapshot Create(string directory, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, System.Threading.CancellationToken cancellationToken)
		{
			directory = directory.IncludeTrailingPathDelimiter();
			var root = new DirectoryInfo(directory);
			if (!root.Exists) throw new DirectoryNotFoundException(directory);
			var regexIgnoreDirectories = ignoreDirectories.CreateIgnoreRegex().ToList();
			var regexIgnoreFiles = ignoreFiles.CreateIgnoreRegex().ToList();

			string ToRelative(string fullName) => fullName[(root.FullName.Length)..];

			Snapshot snapshot = new();
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

		public IEnumerable<KeyValuePair<string, DateTime>> Singles(Snapshot other) => Entries.Where(dir => !other.Entries.ContainsKey(dir.Key));

		public static void Run(string directory, string databaseFileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, System.Threading.CancellationToken cancellationToken)
		{
			Console.WriteLine($"Store state '{directory}' to '{databaseFileName}'");
			var state = Create(directory, ignoreDirectories, ignoreFiles, cancellationToken);
			string json = JsonConvert.SerializeObject(state, Formatting.Indented);
			File.WriteAllText(databaseFileName, json);
		}
	}
}
