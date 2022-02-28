using System.Collections.Generic;
using System;

namespace VersionedCopy
{
	public class SyncOperations
	{
		public SyncOperations(Snapshot mine, Snapshot other, DateTime lastSync)
		{
			static void Split(IEnumerable<KeyValuePair<string, DateTime>> singles, DateTime lastSync, out List<string> newEntries, out List<string> deletedEntries)
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

			Split(mine.Singles(other), lastSync, out var mineNew, out var mineToDelete);
			Split(other.Singles(mine), lastSync, out var otherNew, out var otherToDelete);

			List<string> otherUpdatedFiles = new();
			List<string> mineUpdatedFiles = new();
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
			MineToDelete = mineToDelete.ToArray();
			MineNew = mineNew.ToArray();
			MineUpdatedFiles = mineUpdatedFiles.ToArray();

			OtherToDelete = otherToDelete.ToArray();
			OtherNew = otherNew.ToArray();
			OtherUpdatedFiles = otherUpdatedFiles.ToArray();
		}

		public string[] OtherToDelete { get; }
		public string[] OtherNew { get; }
		public string[] OtherUpdatedFiles { get; }

		public string[] MineToDelete { get; }
		public string[] MineNew { get; }
		public string[] MineUpdatedFiles { get; }
	}
}
