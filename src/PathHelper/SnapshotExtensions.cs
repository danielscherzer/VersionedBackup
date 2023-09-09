using System;
using System.Collections.Generic;
using System.Linq;
using VersionedCopy.Interfaces;

namespace VersionedCopy.PathHelper;

using Entry = KeyValuePair<string, DateTime>;

public static class SnapshotExtensions
{
	public static void FindNewAndToDelete(this Snapshot mine, Snapshot other, DateTime lastSync, out RelativeFileList newEntries, out RelativeFileList toDeleteEntries)
	{
		List<Entry> newEntriesList = new();
		List<Entry> toDeleteEntriesList = new();
		foreach (var single in mine.Singles(other))
		{
			if (single.Value > lastSync)
			{
				newEntriesList.Add(single);
			}
			else
			{
				toDeleteEntriesList.Add(single);
			}
		}
		newEntries = new(mine.Root, newEntriesList);
		toDeleteEntries = new(other.Root, toDeleteEntriesList);
	}

	public static void FindUpdatedFiles(this Snapshot mine, Snapshot other, out RelativeFileList mineUpdatedFiles, out RelativeFileList otherUpdatedFiles)
	{
		List<Entry> mineUpdatedFileList = new();
		List<Entry> otherUpdatedFileList = new();
		foreach (var file in other.Files())
		{
			if (mine.Entries.TryGetValue(file.Key, out var writeTime))
			{
				// files exists in b -> compare time stamp
				var secondsDiff = (file.Value - writeTime).TotalSeconds;
				if (Math.Abs(secondsDiff) < 3) continue;
				if (secondsDiff > 0)
				{
					otherUpdatedFileList.Add(file);
				}
				else //if (secondsDiff < 0)
				{
					mineUpdatedFileList.Add(new Entry(file.Key, writeTime));
				}
			}
		}
		mineUpdatedFiles = new(mine.Root, mineUpdatedFileList);
		otherUpdatedFiles = new(other.Root, otherUpdatedFileList);
	}

	public static RelativeFileList Singles(this Snapshot mine, Snapshot other) => new(mine.Root, mine.Entries.Where(dir => !other.Entries.ContainsKey(dir.Key)));

	private static IRelativeFiles Files(this Snapshot snapshot) => new RelativeFileList(snapshot.Root, snapshot.Entries.Where(entry => Snapshot.IsFile(entry.Key)));
}
