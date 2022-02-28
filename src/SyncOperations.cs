using System.Collections.Generic;
using System;

namespace VersionedCopy
{
	using Directory = KeyValuePair<string, DateTime>;
	using File = KeyValuePair<string, (DateTime creationTime, DateTime writeTime)>;

	public class SyncOperations
	{
		public SyncOperations(Snapshot mine, Snapshot other, DateTime lastSync)
		{
			static void SplitDirectories(IEnumerable<Directory> singles, DateTime lastSync, out List<string> newDirectories, out List<string> deletedDirectories)
			{
				newDirectories = new();
				deletedDirectories = new();
				foreach (var single in singles)
				{
					if (single.Value > lastSync)
					{
						newDirectories.Add(single.Key);
					}
					else
					{
						deletedDirectories.Add(single.Key);
					}
				}
			}

			SplitDirectories(mine.DirectorySingles(other), lastSync, out var mineNewDirectories, out var mineDeletedDirectories);
			SplitDirectories(other.DirectorySingles(mine), lastSync, out var otherNewDirectories, out var otherDeletedDirectories);

			static void SplitFiles(IEnumerable<File> singles, DateTime lastSync, out List<string> newFiles, out List<string> deletedFiles)
			{
				newFiles = new();
				deletedFiles = new();
				foreach (var single in singles)
				{
					if (single.Value.creationTime > lastSync)
					{
						newFiles.Add(single.Key);
					}
					else
					{
						deletedFiles.Add(single.Key);
					}
				}
			}

			SplitFiles(mine.FileSingles(other), lastSync, out var mineNewFiles, out var mineDeletedFiles);
			SplitFiles(other.FileSingles(mine), lastSync, out var otherNewFiles, out var otherDeletedFiles);

			List<string> otherUpdatedFiles = new();
			List<string> mineUpdatedFiles = new();
			foreach (var file in other.Files)
			{
				if (mine.Files.TryGetValue(file.Key, out var myDateTime))
				{
					// files exists in b -> compare write time
					var secondsDiff = (file.Value.writeTime - myDateTime.writeTime).TotalSeconds;
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
