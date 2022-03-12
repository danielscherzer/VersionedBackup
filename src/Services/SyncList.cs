using System;
using System.Collections.Generic;
using System.Linq;

namespace VersionedCopy.Services
{
	internal class SyncList
	{
		public const string FileNameSyncList = ".versioned.copy.sync.list.json";
		private readonly Dictionary<Guid, DateTime> srcSyncs;
		private readonly Dictionary<Guid, DateTime> dstSyncs;
		private readonly string src;
		private readonly string dst;

		public SyncList(string src, string dst)
		{
			srcSyncs = Persist.Load<Dictionary<Guid, DateTime>>(src + FileNameSyncList) ?? new();
			dstSyncs = Persist.Load<Dictionary<Guid, DateTime>>(dst + FileNameSyncList) ?? new();
			var matchingSyncs = srcSyncs.Keys.ToHashSet();
			matchingSyncs.IntersectWith(dstSyncs.Keys);
			if (matchingSyncs.Any())
			{
				LastSyncTime = srcSyncs[matchingSyncs.First()];
				// Remove old sync because new sync will have new guid
				srcSyncs.Remove(matchingSyncs.First());
				dstSyncs.Remove(matchingSyncs.First());
			}
			else
			{
				LastSyncTime = default;
			}
			Guid guid = Guid.NewGuid();
			CurrentSyncTime = DateTime.UtcNow;
			dstSyncs[guid] = CurrentSyncTime;
			srcSyncs[guid] = CurrentSyncTime;

			this.src = src;
			this.dst = dst;
		}

		public DateTime CurrentSyncTime { get; }
		public DateTime LastSyncTime { get; }

		public void Save()
		{
			srcSyncs.Save(src + FileNameSyncList);
			dstSyncs.Save(dst + FileNameSyncList);
		}
	}
}
