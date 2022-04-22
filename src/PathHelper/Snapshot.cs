using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VersionedCopy.Interfaces;
using VersionedCopy.Services;
namespace VersionedCopy.PathHelper
{
	using Entry = KeyValuePair<string, DateTime>;

	public class Snapshot
	{
		public Snapshot(string root)
		{
			Root = Path.GetFullPath(root).IncludeTrailingPathDelimiter();
			BackupDir = GetMetaDataDir(Root);
		}

		public const string CommonFileNamePart = ".versioned.copy";
		public const string FileNameSnapShot = CommonFileNamePart + ".snapshot.json";

		public static string GetMetaDataDir(string path)
		{
			path = path.IncludeTrailingPathDelimiter();
			path = Directory.GetParent(path) is null ? path : path[0..^1];
			return $"{path}{CommonFileNamePart}{Path.DirectorySeparatorChar}";
		}

		public void Add(string name, DateTime writeTime)
		{
			//TODO: Entries.Add(name, Round(writeTime, TimeSpan.FromSeconds(5.0)));
			Entries.Add(name, writeTime);
		}

		public SortedDictionary<string, DateTime> Entries { get; } = new();

		public string FullName(string fileName) => Root + fileName;

		public string BackupName(Entry file)
		{
			var ext = Path.GetExtension(file.Key) ?? string.Empty;
			ext = $".{file.Value:yyyy-MM-dd_HHmmss}{ext}";
			return $"{BackupDir}{Path.ChangeExtension(file.Key, ext)}";
		}

		public string Root { get; private set; }
		
		private string BackupDir { get; }

		public static bool IsFile(string fileName) => !Path.EndsInDirectorySeparator(fileName);

		public IRelativeFiles Files() => new RelativeFileList(Root, Entries.Where(entry => IsFile(entry.Key)));

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

		internal static Snapshot? Load(string dir)
		{
			return Persist.Load<Snapshot>(Path.Combine(GetMetaDataDir(dir), FileNameSnapShot));
		}

		internal void Save()
		{
			var fileName = GetMetaDataDir(Root) + FileNameSnapShot;
			var temp = fileName + ".temp";
			if (File.Exists(temp)) File.Delete(temp);
			this.Save(temp);
			File.Move(temp, fileName, true);
		}

		private static DateTime Round(DateTime dateTime, TimeSpan interval)
		{
			var halfIntervalTicks = (interval.Ticks + 1) >> 1;
			return dateTime.AddTicks(halfIntervalTicks - ((dateTime.Ticks + halfIntervalTicks) % interval.Ticks));
		}
	}
}
