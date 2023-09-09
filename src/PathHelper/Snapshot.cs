using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VersionedCopy.Services;

namespace VersionedCopy.PathHelper;

using Entry = KeyValuePair<string, DateTime>;

public class Snapshot
{
	[JsonConstructor]
	public Snapshot(string root)
	{
		Root = Path.GetFullPath(root).IncludeTrailingPathDelimiter();
		BackupDir = $"{GetMetaDataDir(Root)}{DateTime.Now:yyyy-MM-dd_HHmmss}{Path.DirectorySeparatorChar}";
	}

	public Snapshot(string root, SortedDictionary<string, DateTime> entries) : this(root)
	{
		Entries = entries;
	}

	public const string CommonFileNamePart = ".versioned.copy";
	public const string FileNameSnapShot = CommonFileNamePart + ".snapshot.json";

	public static string GetMetaDataDir(string path)
	{
		path = path.IncludeTrailingPathDelimiter();
		path = Directory.GetParent(path) is null ? path : path[0..^1];
		return $"{path}{CommonFileNamePart}{Path.DirectorySeparatorChar}";
	}

	public SortedDictionary<string, DateTime> Entries { get; } = new();

	public string FullName(string fileName) => Root + fileName;

	public string BackupFileName(Entry file) => BackupDir + file.Key;

	public string Root { get; }

	private string BackupDir { get; }

	public static bool IsFile(string fileName) => !Path.EndsInDirectorySeparator(fileName);

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

		void Add(string name, DateTime writeTime)
		{
			//private static DateTime Round(DateTime dateTime, TimeSpan interval)
			//{
			//	var halfIntervalTicks = (interval.Ticks + 1) >> 1;
			//	return dateTime.AddTicks(halfIntervalTicks - ((dateTime.Ticks + halfIntervalTicks) % interval.Ticks));
			//}
			//TODO: Entries.Add(name, Round(writeTime, TimeSpan.FromSeconds(5.0)));
			snapshot.Entries.Add(name, writeTime);
		}

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
					Add(relativName, file.LastWriteTimeUtc);
				}
				foreach (var subDir in dir.EnumerateDirectories())
				{
					if (cancellationToken.IsCancellationRequested) return snapshot;
					var relativName = ToRelative(subDir.FullName) + Path.DirectorySeparatorChar;
					if (regexIgnoreDirectories.AnyMatch(Path.DirectorySeparatorChar + relativName)) continue;
					Add(relativName, subDir.CreationTimeUtc);
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

	internal static Snapshot? Load(string root)
	{
		var fileName = Path.Combine(GetMetaDataDir(root), FileNameSnapShot);
		var entries = Persist.Load<SortedDictionary<string, DateTime>>(fileName);
		if(entries != null)
		{
			return new Snapshot(root, entries);
		}
		var snapshot = Persist.Load<Snapshot>(fileName);
		return snapshot != null ? new Snapshot(root, snapshot.Entries) : null;
	}

	internal void Save()
	{
		var fileName = GetMetaDataDir(Root) + FileNameSnapShot;
		var temp = fileName + ".temp";
		if (File.Exists(temp)) File.Delete(temp);
		Entries.Save(temp);
		File.Move(temp, fileName, true);
	}
}
