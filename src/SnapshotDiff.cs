using System;
using System.Collections.Generic;
using System.Linq;

namespace VersionedCopy
{
	public class SnapshotDiff
	{
		public SnapshotDiff(Snapshot mine, Snapshot other)
		{
			OtherSingleDirectories = other.DirectorySingles(mine).Select(dir => dir.Key).ToArray();
			MineSingleDirectories = mine.DirectorySingles(other).Select(dir => dir.Key).ToArray();

			OtherSingleFiles = other.FileSingles(mine).Select(file => file.Key).ToArray();
			MineSingleFiles = mine.FileSingles(other).Select(file => file.Key).ToArray();
			List<string> otherNewerFiles = new();
			List<string> mineNewerFiles = new();
			foreach (var file in other.Files)
			{
				if (mine.Files.TryGetValue(file.Key, out var myDateTime))
				{
					// files exists in b -> compare write time
					var secondsDiff = (file.Value.writeTime - myDateTime.writeTime).TotalSeconds;
					if (Math.Abs(secondsDiff) < 3) continue;
					if (secondsDiff > 0)
					{
						otherNewerFiles.Add(file.Key);
					}
					else
					{
						mineNewerFiles.Add(file.Key);
					}
				}
			}
			OtherNewerFiles = otherNewerFiles.ToArray();
			MineNewerFiles = mineNewerFiles.ToArray();
		}

		public string[] OtherSingleDirectories { get; }
		public string[] OtherSingleFiles { get; }
		public string[] OtherNewerFiles { get; }
		public string[] MineSingleDirectories { get; }
		public string[] MineSingleFiles { get; }
		public string[] MineNewerFiles { get; }
	}
}
