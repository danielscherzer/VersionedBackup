using System;
using System.Threading.Tasks;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public static class Mirror
	{
		public static void Run(SrcDstEnv env)
		{
			var src = env.Options.SourceDirectory;
			var dst = env.Options.DestinationDirectory;
			Console.WriteLine($"Mirror from '{src}' to '{dst}'");
			// Create a snapshot from destination
			// Try read snapshot from destination otherwise create
			var taskDst = Task.Run(() => Snapshot.Load(dst) ?? env.CreateSnapshot(dst));
			// Create a snapshot from source
			var taskSrc = Task.Run(() => env.CreateSnapshot(src));
			Task.WaitAll(taskSrc, taskDst);
			var snapSrc = taskSrc.Result;
			var snapDst = taskDst.Result;
			// find singles in source
			var srcSingles = snapSrc.Singles(snapDst);
			// find singles in destination
			var dstSingles = snapDst.Singles(snapSrc);
			env.MoveAway(snapDst, dstSingles);
			//create missing files/directories in dst
			env.Copy(srcSingles, snapDst);
			// Find updated files/directories
			snapSrc.FindUpdatedFiles(snapDst, out var srcUpdatedFiles, out var dstUpdatedFiles);
			// Copy updated files to other side, old version move to old folder, update snapshot
			env.UpdateFiles(srcUpdatedFiles, snapDst);
			// overwrite changed files in destination
			RelativeFileList oldSrcFiles = new(snapSrc.Root, dstUpdatedFiles.Items);
			env.UpdateFiles(oldSrcFiles, snapDst);
			if (!env.ReadOnly)
			{
				//save snapshots with changes
				snapDst.Save();
			}
		}
	}
}
