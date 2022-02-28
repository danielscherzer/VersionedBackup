using System;
using System.Collections.Generic;
using System.Linq;
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

			SyncList syncs = new(src, dst);

			//TODO: Two threads
			// Create a snapshot from source
			var snapSrc = Snapshot.Create(src, env.Options.IgnoreDirectories, env.Options.IgnoreFiles);
			// TODO: try read snapshot from destination otherwise create
			var snapDst = Snapshot.Create(dst, env.Options.IgnoreDirectories, env.Options.IgnoreFiles);
			//var snapDst = Persist.Load<Snapshot>(dst + FileNameSnapShot) ?? Snapshot.Create(dst, env.Options.IgnoreDirectories, env.Options.IgnoreFiles);

			// Diff of source and destination
			SyncOperations diff = new(snapSrc, snapDst, syncs.LastSyncTime);

			// Copy updated files to other side, old version move to old folder
			// TODO: Do on different thread
			foreach (var fileName in diff.MineUpdatedFiles)
			{
				env.FileSystem.MoveFile(dst + fileName, old + fileName); //move away old
				env.FileSystem.Copy(src + fileName, dst + fileName); //copy new
			}
			foreach (var fileName in diff.OtherUpdatedFiles)
			{
				env.FileSystem.MoveFile(src + fileName, old + fileName); //move away old
				env.FileSystem.Copy(dst + fileName, src + fileName); //copy new
			}
			// handle new files/directories
			foreach(var fileName in diff.MineNew)
			{
				if(Snapshot.IsFile(fileName))
				{
					env.FileSystem.Copy(src + fileName, dst + fileName); //copy new
				}
				else
				{
					env.FileSystem.CreateDirectory(dst + fileName);
				}
			}
			foreach (var fileName in diff.OtherNew)
			{
				if (Snapshot.IsFile(fileName))
				{
					env.FileSystem.Copy(dst + fileName, src + fileName); //copy new
				}
				else
				{
					env.FileSystem.CreateDirectory(src + fileName);
				}
			}
			// handle deleted files/directories
			foreach(var fileName in diff.MineToDelete)
			{
				//move deleted to old
				if (Snapshot.IsFile(fileName))
				{
					env.FileSystem.MoveFile(src + fileName, old + fileName);
				}
				else
				{
					env.FileSystem.MoveDirectory(src + fileName, old + fileName);
				}

			}
			foreach (var fileName in diff.OtherToDelete)
			{
				//move deleted to old
				if (Snapshot.IsFile(fileName))
				{
					env.FileSystem.MoveFile(dst + fileName, old + fileName);
				}
				else
				{
					env.FileSystem.MoveDirectory(dst + fileName, old + fileName);
				}

			}
			syncs.Save();
			//TODO: save snapshots with changes at destination
		}
	}
}
