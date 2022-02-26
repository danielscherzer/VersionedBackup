using System;
using System.Collections.Generic;
using System.Linq;

namespace VersionedCopy
{
	public class SnapshotDiff
	{
		public SnapshotDiff(Snapshot other, Snapshot mine)
		{
			DeletedDirectories = other.DirectorySingles(mine).ToArray();
			NewDirectories = mine.DirectorySingles(other).ToArray();

			DeletedFiles = other.FileSingles(mine).ToArray();
			NewFiles = mine.FileSingles(other).ToArray();
			List<string> otherNewer = new();
			List<string> updatedFiles = new();
			foreach (var file in other.Files)
			{
				if (mine.Files.TryGetValue(file.Key, out var myDateTime))
				{
					// files exists in b -> compare write time
					var secondsDiff = (file.Value - myDateTime).TotalSeconds;
					if (Math.Abs(secondsDiff) < 3) continue;
					if (secondsDiff > 0)
					{
						otherNewer.Add(file.Key);
					}
					else
					{
						updatedFiles.Add(file.Key);
					}
				}
			}
			OtherNewerFiles = otherNewer.ToArray();
			UpdatedFiles = updatedFiles.ToArray();
		}

		public string[] DeletedDirectories { get; }
		public string[] DeletedFiles { get; }
		public string[] OtherNewerFiles { get; }
		public string[] NewDirectories { get; }
		public string[] NewFiles { get; }
		public string[] UpdatedFiles { get; }
	}
}
