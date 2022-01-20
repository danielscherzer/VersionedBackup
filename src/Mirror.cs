using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;

namespace VersionedCopy
{
	public class Mirror : Algorithm
	{
		public Mirror(IOptions options, IReport report, IFileSystem fileSystem, CancellationToken token) : base(options, report, fileSystem, token)
		{
			//create missing directories in dst
			var newDirs = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in newDirs)
			{
				if (token.IsCancellationRequested) return;
				op.CreateDirectory(subDir);
			}

			// make sure dst enumeration task has ended before changing directories and files
			dstFilesRelative.Wait(token);

			// find directories in dst, but not in src
			var dirsToMove = dstDirsRelative.Where(dstDir => !srcDirsRelative.Contains(dstDir));
			// move away old directories
			foreach (var subDir in dirsToMove)
			{
				if (token.IsCancellationRequested) return;
				op.MoveAwayDeletedDir(subDir);
			}
			// find files in dst, but not in src
			var filesToMove = dstFilesRelative.Result.Where(dstFileRelative => !srcFilesRelative.Result.Contains(dstFileRelative));
			foreach (var fileName in filesToMove)
			{
				if (token.IsCancellationRequested) return;
				op.MoveAwayDeletedFile(fileName);
			}

			try
			{
				Parallel.ForEach(srcFilesRelative.Result, new ParallelOptions { CancellationToken = token }, fileName =>
				{
					if (dstFilesRelative.Result.Contains(fileName))
					{
						op.ReplaceChangedFile(fileName);
					}
					else
					{
						op.CopyNewFile(fileName);
					}
				});
			}
			catch (OperationCanceledException)
			{
			}
		}
	}
}
