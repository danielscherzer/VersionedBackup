using System.Collections.Generic;
using System;

namespace VersionedCopy
{
	using Entry = KeyValuePair<string, DateTime>;

	public class SyncOperations
	{
		public SyncOperations(Snapshot mine, Snapshot other, DateTime lastSync)
		{
			static void Split(IEnumerable<Entry> singles, DateTime lastSync, out List<string> newEntries, out List<string> deletedEntries)
			{
				newEntries = new();
				deletedEntries = new();
				foreach (var single in singles)
				{
					if (single.Value > lastSync)
					{
						newEntries.Add(single.Key);
					}
					else
					{
						deletedEntries.Add(single.Key);
					}
				}
			}

			Split(mine.DirectorySingles(other), lastSync, out var mineNewDirectories, out var mineDeletedDirectories);
			Split(other.DirectorySingles(mine), lastSync, out var otherNewDirectories, out var otherDeletedDirectories);
			Split(mine.FileSingles(other), lastSync, out var mineNewFiles, out var mineDeletedFiles);
			Split(other.FileSingles(mine), lastSync, out var otherNewFiles, out var otherDeletedFiles);

			List<string> otherUpdatedFiles = new();
			List<string> mineUpdatedFiles = new();
			foreach (var file in other.Files)
			{
				if (mine.Files.TryGetValue(file.Key, out var writeTime))
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
			OtherUpdatedFiles = otherUpdatedFiles.ToArray();
			MineUpdatedFiles = mineUpdatedFiles.ToArray();
		}

		public string[] OtherUpdatedFiles { get; }
		public string[] MineUpdatedFiles { get; }
	}
}
