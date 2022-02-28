using System;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public class NewSync
	{
		public const string FileNameDeleteHistory = ".versioned.copy.deleteHistory.json";
		public const string FileNameSnapShot = ".versioned.copy.snapshot.json";
		public static void Run(AlgorithmEnv env)
		{
			//TODO: respect cancellation token
			var src = env.Options.SourceDirectory.IncludeTrailingPathDelimiter();
			var dst = env.Options.DestinationDirectory.IncludeTrailingPathDelimiter();
			var old = env.Options.OldFilesFolder.IncludeTrailingPathDelimiter();
			Console.WriteLine($"NewSync from '{src}' to '{dst}'");
			//TODO: Two threads
			// Create a snapshot from source
			var snapSrc = Snapshot.Create(src, env.Options.IgnoreDirectories, env.Options.IgnoreFiles);
			// TODO: try read snapshot from destination otherwise create
			var snapDst = Persist.Load<Snapshot>(dst + FileNameSnapShot) ?? Snapshot.Create(dst, env.Options.IgnoreDirectories, env.Options.IgnoreFiles);
			// Diff of source and destination
			SnapshotDiff diff = new(snapSrc, snapDst);
			// Copy updated files to other side, old version move to old folder
			// TODO: Do on different thread
			foreach (var fileName in diff.MineNewerFiles)
			{
				env.FileSystem.MoveFile(dst + fileName, old + fileName); //move away old
				env.FileSystem.Copy(src + fileName, dst + fileName); //copy new
			}
			foreach (var fileName in diff.OtherNewerFiles)
			{
				env.FileSystem.MoveFile(src + fileName, old + fileName); //move away old
				env.FileSystem.Copy(dst + fileName, src + fileName); //copy new
			}
			//Handling of single/orphan files/directories on both sides

			//Try load sourc delete history from disc
			DeleteHistory deleteHistory = Persist.Load<DeleteHistory>(src + FileNameDeleteHistory) ?? new();
			//Update delete history with changes from old and source snapshot
			deleteHistory.Update(Persist.Load<Snapshot>(src + FileNameSnapShot) ?? snapSrc, snapSrc);
			//Save updated delete history
			deleteHistory.Save(src + FileNameDeleteHistory);
			//What is deleted in the delete history should be checked for deletion on destination side
			//foreach(var fileName in diff.)

			snapSrc.Save(src + FileNameSnapShot);
			//TODO: save snapshots with changes
		}
	}
}
