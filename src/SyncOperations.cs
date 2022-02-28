using System.Collections.Generic;
using System;

namespace VersionedCopy
{
	public static class SyncOperations
	{
		public static void NewAndToDelete(Snapshot mine, Snapshot other, DateTime lastSync, out List<string> newEntries, out List<string> toDeleteEntries)
		{
			newEntries = new();
			toDeleteEntries = new();
			foreach (var single in mine.Singles(other))
			{
				if (single.Value > lastSync)
				{
					newEntries.Add(single.Key);
				}
				else
				{
					toDeleteEntries.Add(single.Key);
				}
			}
		}

		public static void UpdatedFiles(Snapshot mine, Snapshot other, out List<string> mineUpdatedFiles, out List<string> otherUpdatedFiles)
		{
			mineUpdatedFiles = new();
			otherUpdatedFiles = new();
			foreach (var file in other.Files())
			{
				if (mine.Entries.TryGetValue(file.Key, out var writeTime))
				{
					// files exists in b -> compare write time
					var secondsDiff = (file.Value - writeTime).TotalSeconds;
					if (Math.Abs(secondsDiff) < 3) continue;
					if (secondsDiff > 0)
					{
						otherUpdatedFiles.Add(file.Key);
					}
					else
					{
						mineUpdatedFiles.Add(file.Key);
					}
				}
			}
		}
	}
}
