using System;
using System.Linq;
using System.Threading.Tasks;
using VersionedCopy.PathHelper;

namespace VersionedCopy
{
	public class Sync
	{
		public static void Run(AlgorithmEnv env)
		{
			var src = env.Options.SourceDirectory.IncludeTrailingPathDelimiter();
			var dst = env.Options.DestinationDirectory.IncludeTrailingPathDelimiter();

			Task<string[]> srcDirs = env.EnumerateDirsAsync(src);
			Task<string[]> dstDirs = env.EnumerateDirsAsync(dst);

			var srcDirsRelative = srcDirs.Result.ToRelative(src).ToHashSet();
			var dstDirsRelative = dstDirs.Result.ToRelative(dst).ToHashSet();

			var srcFilesRelative = env.EnumerateRelativeFilesAsync(src, srcDirs.Result);
			var dstFilesRelative = env.EnumerateRelativeFilesAsync(dst, dstDirs.Result);

			//create missing directories in dst
			var srcDirSingles = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in srcDirSingles)
			{
				if (env.Token.IsCancellationRequested) return;
				//TODO: if in delete log move to old dir
				env.Op.CreateDirectory(subDir);
			}

			// make sure dst enumeration task has ended before changing directories and files
			dstFilesRelative.Wait(env.Token);

			// find directories in dst, but not in src
			var dstDirSingles = dstDirsRelative.Where(dstDir => !srcDirsRelative.Contains(dstDir));
			// create missing directories in src
			foreach (var subDir in dstDirSingles)
			{
				if (env.Token.IsCancellationRequested) return;
				//TODO: if in delete log move to old dir
				env.Op.CreateSrcDirectory(subDir);
			}

			// find files in src, but not in dst
			var srcFileSingles = srcFilesRelative.Result.Where(srcFileRelative => !dstFilesRelative.Result.Contains(srcFileRelative));
			foreach (var fileName in srcFileSingles)
			{
				if (env.Token.IsCancellationRequested) return;
				env.Op.CopyNewFile(fileName);
				//TODO: if in delete log op.MoveAwayDeleted(fileName);
			}

			// find files in dst, but not in src
			var dstFileSingles = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative));
			foreach (var fileName in dstFileSingles)
			{
				if (env.Token.IsCancellationRequested) return;
				env.Op.CopyNewFileToSrc(fileName);
				//TODO: if in delete log op.MoveAwayDeleted(fileName);
			}

			// in both
			srcFilesRelative.Result.IntersectWith(dstFilesRelative.Result);
			try
			{
				//Parallel.ForEach(srcFilesRelative.Result, new ParallelOptions { CancellationToken = env.Token }, fileName =>
				foreach (var fileName in srcFilesRelative.Result)
				{
					env.Op.CopyNewerFileToOtherSide(fileName);
				}//);
			}
			catch (OperationCanceledException)
			{
			}
		}
	}
}
