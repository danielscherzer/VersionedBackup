using System;
using System.Collections.Generic;
using VersionedCopy.PathHelper;

namespace VersionedCopy
{
	using Entry = KeyValuePair<string, DateTime>;

	public static class SyncOperations
	{
		public static void FindNewAndToDelete(Snapshot mine, Snapshot other, DateTime lastSync, out List<Entry> newEntries, out List<Entry> toDeleteEntries)
		{
			newEntries = new();
			toDeleteEntries = new();
			foreach (var single in mine.Singles(other))
			{
				if (single.Value > lastSync)
				{
					newEntries.Add(single);
				}
				else
				{
					toDeleteEntries.Add(single);
				}
			}
		}

		public static void FindUpdatedFiles(Snapshot mine, Snapshot other, out List<Entry> mineUpdatedFiles, out List<Entry> otherUpdatedFiles)
		{
			mineUpdatedFiles = new();
			otherUpdatedFiles = new();
			foreach (var file in other.Files())
			{
				if (mine.Entries.TryGetValue(file.Key, out var writeTime))
				{
					// files exists in b -> compare time stamp
					var secondsDiff = (file.Value - writeTime).TotalSeconds;
					if (Math.Abs(secondsDiff) < 3) continue;
					if (secondsDiff > 0)
					{
						otherUpdatedFiles.Add(file);
					}
					else
					{
						mineUpdatedFiles.Add(new Entry(file.Key, writeTime));
					}
				}
			}
		}
	}
}
