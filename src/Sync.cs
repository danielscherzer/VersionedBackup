using System;
using System.IO;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public class Sync
	{
		public const string FileNameSnapShot = ".versioned.copy.snapshot.json";
		
		public static void Run(AlgorithmEnv env)
		{
			var src = env.Options.SourceDirectory.IncludeTrailingPathDelimiter();
			var dst = env.Options.DestinationDirectory.IncludeTrailingPathDelimiter();
			if (!Directory.Exists(dst)) env.Op.CreateDirectory(".");
			Console.WriteLine($"Sync '{src}' <-> '{dst}'");

			//TODO: Two threads
			// Create a snapshot from source
			var snapSrc = Snapshot.Create(src, env.Options.IgnoreDirectories, env.Options.IgnoreFiles, env.Token);
			// TODO: try read snapshot from destination otherwise create
			var snapDst = Persist.Load<Snapshot>(dst + FileNameSnapShot) ?? Snapshot.Create(dst, env.Options.IgnoreDirectories, env.Options.IgnoreFiles, env.Token);

			// Copy updated files to other side, old version move to old folder
			SyncOperations.UpdatedFiles(snapSrc, snapDst, out var srcUpdatedFiles, out var dstUpdatedFiles);
			// TODO: Do on different thread
			env.UpdateFiles(src, dst, srcUpdatedFiles);
			env.UpdateFiles(dst, src, dstUpdatedFiles);

			SyncList syncs = new(src, dst);

			SyncOperations.NewAndToDelete(snapSrc, snapDst, syncs.LastSyncTime, out var srcNew, out var srcToDelete);
			env.Copy(src, dst, srcNew);
			env.MoveAway(src, srcToDelete);

			SyncOperations.NewAndToDelete(snapDst, snapSrc, syncs.LastSyncTime, out var dstNew, out var dstToDelete);
			env.Copy(dst, src, dstNew);
			env.MoveAway(dst, dstToDelete);
			
			syncs.Save();
			//TODO: save snapshots with changes at destination
		}
	}
}
