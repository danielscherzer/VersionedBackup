using System;
using System.Collections.Generic;
using System.Linq;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services;

internal class SyncList
{
	public const string FileNameSyncList = Snapshot.CommonFileNamePart + ".sync.list.json";
	private readonly Dictionary<Guid, DateTime> srcSyncs;
	private readonly Dictionary<Guid, DateTime> dstSyncs;
	private readonly string src;
	private readonly string dst;

	public SyncList(string src, string dst)
	{
		this.src = Snapshot.GetMetaDataDir(src) + FileNameSyncList;
		this.dst = Snapshot.GetMetaDataDir(dst) + FileNameSyncList;
		srcSyncs = Persist.Load<Dictionary<Guid, DateTime>>(this.src) ?? new();
		dstSyncs = Persist.Load<Dictionary<Guid, DateTime>>(this.dst) ?? new();
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
	}

	public DateTime CurrentSyncTime { get; }
	public DateTime LastSyncTime { get; }

	public void Save()
	{
		srcSyncs.Save(src);
		dstSyncs.Save(dst);
	}
}
