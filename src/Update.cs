using System.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;

namespace VersionedCopy
{
	public class Update : Algorithm
	{
		public Update(IOptions options, IReport report, IFileSystem fileSystem, CancellationToken token) : base(options, report, fileSystem, token)
		{
			//create missing directories in dst
			var newDirs = srcDirsRelative.Where(srcDir => !dstDirsRelative.Contains(srcDir));
			foreach (var subDir in newDirs)
			{
				if (token.IsCancellationRequested) return;
				op.CreateDirectory(subDir);
			}

			// make sure dst enumeration task has ended before changing files
			dstFilesRelative.Wait(token);

			try
			{
				Parallel.ForEach(srcFilesRelative.Result, new ParallelOptions { CancellationToken = token }, fileName =>
				{
					if (dstFilesRelative.Result.Contains(fileName))
					{
						op.CopyUpdatedFile(fileName);
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
