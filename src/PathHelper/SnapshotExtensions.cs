using System;
using System.Collections.Generic;

namespace VersionedCopy.PathHelper
{
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
					else
					{
						mineUpdatedFileList.Add(new Entry(file.Key, writeTime));
					}
				}
			}
			mineUpdatedFiles = new(mine.Root, mineUpdatedFileList);
			otherUpdatedFiles = new(other.Root, otherUpdatedFileList);
		}
	}
}
