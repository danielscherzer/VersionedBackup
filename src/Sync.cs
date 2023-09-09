using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy;

public class Sync
{
	public static void Run(SrcDstEnv env)
	{
		var src = env.Options.SourceDirectory;
		var dst = env.Options.DestinationDirectory;
		env.Output.Report($"Sync '{src}' <-> '{dst}'");
		Stopwatch time = Stopwatch.StartNew();

		// Try read snapshot from destination otherwise create
		var taskDst = Task.Run(() => Snapshot.Load(dst) ?? env.CreateSnapshot(dst));
		// Create a snapshot from source
		var taskSrc = Task.Run(() => env.CreateSnapshot(src));
		// try load old snapshot from source
		var taskSrcOld = Task.Run(() => Snapshot.Load(src));
		Task.WaitAll(taskSrc, taskDst);
		var snapSrc = taskSrc.Result;
		var snapDst = taskDst.Result;
		time.Benchmark("Snapshots");

		SyncList syncs = new(src, dst);
		if (taskSrcOld.Result is Snapshot snapOld)
		{
			// history present: 2 cases problematic:
			// changed were older has newer time stamp (e.x.: resurrected file) -> set time stamp old + 5 sec
			snapSrc.FindUpdatedFiles(snapOld, out var _, out var oldUpdatedFiles);
			foreach (var file in oldUpdatedFiles)
			{
				var newTime = file.Value.AddSeconds(5);
				var fileName = file.Key;
				snapSrc.Entries[fileName] = newTime;
				env.SetTimeStamp(snapSrc.FullName(fileName), newTime);
			}
			// new with time stamp older last sync (e.x.: rename) -> set time current sync + 5 sec
			var newCreated = snapSrc.Singles(snapOld).ToList();
			foreach (var file in newCreated)
			{
				if (file.Value < syncs.LastSyncTime)
				{
					var newTime = syncs.CurrentSyncTime.AddSeconds(5);
					var fileName = file.Key;
					snapSrc.Entries[fileName] = newTime;
					env.SetTimeStamp(snapSrc.FullName(fileName), newTime);
				}
			}
		}

		// Find updated files/directories
		snapSrc.FindUpdatedFiles(snapDst, out var srcUpdatedFiles, out var dstUpdatedFiles);
		snapSrc.FindNewAndToDelete(snapDst, syncs.LastSyncTime, out var srcNew, out var srcToDelete);
		snapDst.FindNewAndToDelete(snapSrc, syncs.LastSyncTime, out var dstNew, out var dstToDelete);
		time.Benchmark("Create lists");
		if (!(srcUpdatedFiles.Any() || dstUpdatedFiles.Any() || srcNew.Any() || dstNew.Any() || srcToDelete.Any() || dstToDelete.Any()))
		{
			env.Output.Report("Everything up-to-date");
			return;
		}
		var debugString = $"srcUpd({srcUpdatedFiles}), dstUpd({dstUpdatedFiles}), " +
			$"srcNew({srcNew}), dstNew({dstNew}), srcDel({srcToDelete}), dstDel({dstToDelete})";
		// move away before copy because file with only capitalisation differences could exist after rename
		env.MoveAway(snapSrc, srcToDelete);
		env.MoveAway(snapDst, dstToDelete);

		env.Copy(srcNew, snapDst);
		env.Copy(dstNew, snapSrc);

		// Copy updated files to other side, old version move to old folder, update snapshot
		env.UpdateFiles(srcUpdatedFiles, snapDst); // TODO: Do on different thread
		env.UpdateFiles(dstUpdatedFiles, snapSrc);

		time.Benchmark("Copy");

		if (!env.ReadOnly)
		{
			//save snapshots with changes
			snapDst.Save();
			time.Benchmark("snapshot dst save");
			snapSrc.Save();
			time.Benchmark("snapshot src save");
			if (!env.Token.IsCancellationRequested)
			{
				syncs.Save();
				time.Benchmark("Sync save");
			}
		}
	}
}
