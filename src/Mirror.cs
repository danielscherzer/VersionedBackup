using System;
using System.Linq;
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

			Task<string[]> srcDirs = env.EnumerateDirsAsync(src);
			Task<string[]> dstDirs = env.EnumerateDirsAsync(dst);

			var srcDirsRelative = srcDirs.Result.ToRelative(src).ToHashSet();
			var dstDirsRelative = dstDirs.Result.ToRelative(dst).ToHashSet();

			var srcFilesRelative = env.EnumerateRelativeFilesAsync(src, srcDirs.Result);
			var dstFilesRelative = env.EnumerateRelativeFilesAsync(dst, dstDirs.Result);

			//create missing directories in dst
			var newDirs = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in newDirs)
			{
				if (env.Token.IsCancellationRequested) return;
				env.Op.CreateDirectory(subDir);
			}

			// make sure dst enumeration task has ended before changing directories and files
			dstFilesRelative.Wait(env.Token);

			// find directories in dst, but not in src
			var dirsToMove = dstDirsRelative.Where(dstDir => !srcDirsRelative.Contains(dstDir));
			// move away old directories
			foreach (var subDir in dirsToMove)
			{
				if (env.Token.IsCancellationRequested) return;
				env.Op.MoveAwayDeletedDir(subDir);
			}
			// find files in dst, but not in src
			var filesToMove = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative));
			foreach (var fileName in filesToMove)
			{
				if (env.Token.IsCancellationRequested) return;
				env.Op.MoveAwayDeletedFile(fileName);
			}

			try
			{
				Parallel.ForEach(srcFilesRelative.Result, new ParallelOptions { CancellationToken = env.Token }, fileName =>
				{
					if (dstFilesRelative.Result.Contains(fileName))
					{
						env.Op.ReplaceChangedFile(fileName);
					}
					else
					{
						env.Op.CopyNewFile(fileName);
					}
				});
			}
			catch (OperationCanceledException)
			{
			}
		}
	}
}
