using System;
using System.Threading.Tasks;
using VersionedCopy.PathHelper;

namespace VersionedCopy
{
	public static class Mirror
	{
		public static void Run(AlgorithmEnv env)
		{
			var src = env.Options.SourceDirectory.IncludeTrailingPathDelimiter();
			var dst = env.Options.DestinationDirectory.IncludeTrailingPathDelimiter();
			Console.WriteLine($"Mirror from '{src}' to '{dst}'");
			// Create a snapshot from destination
			var taskDst = Task.Run(() => Snapshot.Create(dst, env.Options.IgnoreDirectories, env.Options.IgnoreFiles, env.Token));
			// Create a snapshot from source
			var taskSrc = Task.Run(() => Snapshot.Create(src, env.Options.IgnoreDirectories, env.Options.IgnoreFiles, env.Token));
			Task.WaitAll(taskSrc, taskDst);
			var snapSrc = taskSrc.Result;
			var snapDst = taskDst.Result;
			// find singles in source
			var srcSingles = snapSrc.Singles(snapDst);
			// find singles in destination
			var dstSingles = snapDst.Singles(snapSrc);
			env.MoveAway(dst, dstSingles, snapDst);
			//create missing files/directories in dst
			env.Copy(src, dst, srcSingles);
			// Find updated files/directories
			SyncOperations.FindUpdatedFiles(snapSrc, snapDst, out var srcUpdatedFiles, out var dstUpdatedFiles);
			// Copy updated files to other side, old version move to old folder, update snapshot
			env.UpdateFiles(src, dst, srcUpdatedFiles);
			// overwrite changed files in destination
			env.UpdateFiles(src, dst, dstUpdatedFiles);
		}
	}
}
