using System;
using System.Collections.Generic;
using System.Linq;
using VersionedCopy.Services;

namespace VersionedCopy
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
				// TODO: remove old sync because
				srcSyncs.Remove(matchingSyncs.First());
				dstSyncs.Remove(matchingSyncs.First());
			}
			else
			{
				LastSyncTime = default;
			}

			this.src = src;
			this.dst = dst;
		}

		public DateTime LastSyncTime { get; }

		//TODO: Dissallow to call mutliple times _> Dipsosed
		public void Save()
		{
			Guid guid = Guid.NewGuid();
			var timeStamp = DateTime.UtcNow;
			dstSyncs[guid] = timeStamp;
			srcSyncs[guid] = timeStamp;
			Persist.Save(srcSyncs, src + FileNameSyncList);
			Persist.Save(dstSyncs, dst + FileNameSyncList);
		}
	}
}
