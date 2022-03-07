using System;
using System.Diagnostics;
using System.Threading.Tasks;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public class Sync
	{
		public static void Run(AlgorithmEnv env)
		{
			var src = env.Options.SourceDirectory.IncludeTrailingPathDelimiter();
			var dst = env.Options.DestinationDirectory.IncludeTrailingPathDelimiter();
			Console.WriteLine($"Sync '{src}' <-> '{dst}'");

			Stopwatch time = Stopwatch.StartNew();
			// Try read snapshot from destination otherwise create
			var taskDst = Task.Run(() => AlgorithmEnv.LoadSnapshot(dst) ?? env.CreateSnapshot(dst));
			// Create a snapshot from source
			var taskSrc = Task.Run(() => env.CreateSnapshot(src));
			// try load old snapshort from source
			var taskSrcOld = Task.Run(() => AlgorithmEnv.LoadSnapshot(src));
			Task.WaitAll(taskSrc, taskDst);
			var snapSrc = taskSrc.Result;
			var snapDst = taskDst.Result;
			time.Benchmark("Snapshots");

			SyncList syncs = new(src, dst);
			if (taskSrcOld.Result is Snapshot snapOld)
			{
				// history present: 2 cases problematic:
				// changed were older has newer time stamp (e.x.: resurrected file) -> set time stamp old + 5 sec
				SyncOperations.FindUpdatedFiles(snapSrc, snapOld, out var _, out var oldUpdatedFiles);
				foreach (var file in oldUpdatedFiles)
				{
					var newTime = file.Value.AddSeconds(5);
					var fileName = file.Key;
					snapSrc.Entries[fileName] = newTime;
					env.SetTimeStamp(src + fileName, newTime);
				}
				// new with time stamp older last sync (e.x.: rename) -> set time current sync + 5 sec
				var newCreated = snapSrc.Singles(snapOld);
				foreach (var file in newCreated)
				{
					if (file.Value < syncs.LastSyncTime)
					{
						var fileName = file.Key;
						var newTime = syncs.CurrentSyncTime.AddSeconds(5);
						snapSrc.Entries[file.Key] = newTime;
						env.SetTimeStamp(src + fileName, newTime);
					}
				}
			}

			// Find updated files/directories
			SyncOperations.FindUpdatedFiles(snapSrc, snapDst, out var srcUpdatedFiles, out var dstUpdatedFiles);
			SyncOperations.FindNewAndToDelete(snapSrc, snapDst, syncs.LastSyncTime, out var srcNew, out var srcToDelete);
			SyncOperations.FindNewAndToDelete(snapDst, snapSrc, syncs.LastSyncTime, out var dstNew, out var dstToDelete);
			time.Benchmark("Create lists");

			// move away before copy because file with only capitalisation differences could exist after rename
			env.MoveAway(src, srcToDelete, snapSrc);
			env.MoveAway(dst, dstToDelete, snapDst);

			env.Copy(src, dst, srcNew, snapDst);
			env.Copy(dst, src, dstNew, snapSrc);

			// Copy updated files to other side, old version move to old folder, update snapshot
			env.UpdateFiles(src, dst, srcUpdatedFiles, snapDst); // TODO: Do on different thread
			env.UpdateFiles(dst, src, dstUpdatedFiles, snapSrc);

			time.Benchmark("Copy");

			if (!env.Options.ReadOnly)
			{
				syncs.Save();
				time.Benchmark("Sync save");
				//save snapshots with changes
				AlgorithmEnv.SaveSnapshot(snapSrc, src);
				time.Benchmark("snapshot src save");
				AlgorithmEnv.SaveSnapshot(snapDst, dst);
				time.Benchmark("snapshot dst save");
			}
		}
	}
}
