using System;
using System.Threading.Tasks;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public class Update
	{
		public static void Run(SrcDstEnv env)
		{
			var src = env.Options.SourceDirectory;
			var dst = env.Options.DestinationDirectory;
			Console.WriteLine($"Update from '{src}' to '{dst}'");
			// Try read snapshot from destination otherwise create
			var taskDst = Task.Run(() => env.CreateSnapshot(dst));
			// Create a snapshot from source
			var taskSrc = Task.Run(() => env.CreateSnapshot(src));
			Task.WaitAll(taskSrc, taskDst);
			var snapSrc = taskSrc.Result;
			var snapDst = taskDst.Result;
			// find singles in source
			var srcSingles = snapSrc.Singles(snapDst);
			//create missing files/directories in dst
			env.Copy(srcSingles, snapDst);
			// Find updated files/directories
			snapSrc.FindUpdatedFiles(snapDst, out var srcUpdatedFiles, out var _);
			// Copy updated files to other side, old version move to old folder, update snapshot
			env.UpdateFiles(srcUpdatedFiles, snapDst);
			if (!env.ReadOnly)
			{
				//save snapshots with changes
				snapDst.Save();
			}
		}
	}
}
