using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VersionedCopy.PathHelper;

namespace VersionedCopy
{
	using Directory = KeyValuePair<string, DateTime>;
	using File = KeyValuePair<string, (DateTime creationTime, DateTime writeTime)>;

	public class Snapshot
	{
		public Snapshot()
		{
			TimeStamp = DateTime.UtcNow;
		}

		public Snapshot(DateTime timeStamp)
		{
			TimeStamp = timeStamp;
		}

		public Dictionary<string, DateTime> Directories { get; } = new ();
		public Dictionary<string, (DateTime creationTime, DateTime writeTime)> Files { get; } = new();
		public DateTime TimeStamp { get; set; }

		public void AddFile(string name, DateTime creationTime, DateTime writeTime) => Files.Add(name, (creationTime, writeTime));

		//public static Snapshot Create(string directory, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		//{
		//	//TODO: add cancellation token
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

		public static Snapshot Create(string directory, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		{
			//TODO: add cancellation token
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
						var relativName = ToRelative(file.FullName);
						if (regexIgnoreFiles.AnyMatch(relativName)) continue;
						snapshot.AddFile(relativName, file.CreationTimeUtc, file.LastWriteTimeUtc);
					}
					foreach (var subDir in dir.EnumerateDirectories())
{
						var relativName = ToRelative(subDir.FullName) + Path.DirectorySeparatorChar;
						if (regexIgnoreDirectories.AnyMatch(relativName)) continue;
						snapshot.Directories[relativName] = subDir.CreationTimeUtc;
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

		public IEnumerable<Directory> DirectorySingles(Snapshot other) => Directories.Where(dir => !other.Directories.ContainsKey(dir.Key));

		public IEnumerable<File> FileSingles(Snapshot other) => Files.Where(file => !other.Files.ContainsKey(file.Key));

		public static void Run(string directory, string databaseFileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		{
			Console.WriteLine($"Store state '{directory}' to '{databaseFileName}'");
			var state = Create(directory, ignoreDirectories, ignoreFiles);
			string json = JsonConvert.SerializeObject(state, Formatting.Indented);
			System.IO.File.WriteAllText(databaseFileName, json);
		}
	}
}
