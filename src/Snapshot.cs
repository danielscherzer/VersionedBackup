using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VersionedCopy.PathHelper;

namespace VersionedCopy
{
	public class Snapshot
	{
		public HashSet<string> Directories { get; set; }
		public Dictionary<string, DateTime> Files { get; }
		public DateTime TimeStamp { get; }

		public Snapshot(HashSet<string> dirs, Dictionary<string, DateTime> files, DateTime timeStamp)
		{
			Directories = dirs;
			Files = files;
			TimeStamp = timeStamp;
		}

		public static Snapshot Create(string directory, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		{
			//TODO: add cancellation token
			directory = directory.IncludeTrailingPathDelimiter();
			var dirs = Directory.GetDirectories(directory, "*.*", SearchOption.AllDirectories)
				.ToRelative(directory)
				.Select(dir => dir.IncludeTrailingPathDelimiter())
				.Ignore(ignoreDirectories);

			var dirHash = dirs.ToHashSet();
			var regexIgnoreFiles = ignoreFiles.CreateIgnoreRegex().ToList();

			var files = from subDir in dirHash
						from file in Directory.EnumerateFiles(directory + subDir)
						where !regexIgnoreFiles.AnyMatch(file)
						select file;

			var fileInfo = files.ToDictionary(file => file[directory.Length..], file => File.GetLastWriteTimeUtc(file));
			return new Snapshot(dirHash, fileInfo, DateTime.Now);
		}

		public IEnumerable<string> DirectorySingles(Snapshot other) => Directories.Where(dir => !other.Directories.Contains(dir));

		public IEnumerable<string> FileSingles(Snapshot other) => Files.Where(file => !other.Files.ContainsKey(file.Key)).Select(file => file.Key);

		public static void Run(string directory, string databaseFileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		{
			Console.WriteLine($"Store state '{directory}' to '{databaseFileName}'");
			var state = Create(directory, ignoreDirectories, ignoreFiles);
			string json = JsonConvert.SerializeObject(state, Formatting.Indented);
			File.WriteAllText(databaseFileName, json);
		}
	}
}
