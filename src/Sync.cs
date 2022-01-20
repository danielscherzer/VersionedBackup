using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;

namespace VersionedCopy
{
	public class Sync : Algorithm
	{
		public Sync(IOptions options, IReport report, IFileSystem fileSystem, CancellationToken token) : base(options, report, fileSystem, token)
		{
			//create missing directories in dst
			var srcDirSingles = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in srcDirSingles)
			{
				if (token.IsCancellationRequested) return;
				//TODO: if in delete log move to old dir
				op.CreateDirectory(subDir);
			}

			// make sure dst enumeration task has ended before changing directories and files
			dstFilesRelative.Wait(token);

			// find directories in dst, but not in src
			var dstDirSingles = dstDirsRelative.Where(dstDir => !srcDirsRelative.Contains(dstDir));
			// create missing directories in src
			foreach (var subDir in dstDirSingles)
			{
				if (token.IsCancellationRequested) return;
				//TODO: if in delete log move to old dir
				op.CreateSrcDirectory(subDir);
			}

			// find files in src, but not in dst
			var srcFileSingles = srcFilesRelative.Result.Where(srcFileRelative => !dstFilesRelative.Result.Contains(srcFileRelative));
			foreach (var fileName in srcFileSingles)
			{
				if (token.IsCancellationRequested) return;
				op.CopyNewFile(fileName);
				//TODO: if in delete log op.MoveAwayDeleted(fileName);
			}

			// find files in dst, but not in src
			var dstFileSingles = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative));
			foreach (var fileName in dstFileSingles)
			{
				if (token.IsCancellationRequested) return;
				op.CopyNewFileToSrc(fileName);
				//TODO: if in delete log op.MoveAwayDeleted(fileName);
			}

			// in both
			srcFilesRelative.Result.IntersectWith(dstFilesRelative.Result);
			try
			{
				Parallel.ForEach(srcFilesRelative.Result, new ParallelOptions { CancellationToken = token }, fileName =>
				{
					op.CopyNewerFileToOtherSide(fileName);
				});
			}
			catch (OperationCanceledException)
			{
			}
		}
	}
}
