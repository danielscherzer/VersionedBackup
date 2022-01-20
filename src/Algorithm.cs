using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public class Algorithm
	{
		protected readonly HashSet<string> srcDirsRelative;
		protected readonly HashSet<string> dstDirsRelative;
		protected readonly Task<HashSet<string>> srcFilesRelative;
		protected readonly Task<HashSet<string>> dstFilesRelative;
		protected readonly CancellationToken token;
		protected readonly FileSystemOperations op;

		/// <summary>
		/// optimize common update case: assume two directory structures are very similar
		/// 1=> read multi-threaded all directories and then all files
		/// 2=> optimize compare of almost all files for date time and size
		/// </summary>
		/// <param name="options"></param>
		/// <param name="token"><see cref="CancellationToken"/></param>
		public Algorithm(IOptions options, IReport report, IFileSystem fileSystem, CancellationToken token)
		{
			op = new FileSystemOperations(report, options, fileSystem);
			this.token = token;

			var src = options.SourceDirectory.IncludeTrailingPathDelimiter();
			var dst = options.DestinationDirectory.IncludeTrailingPathDelimiter();

			if (!fileSystem.ExistsDirectory(src)) op.CreateSrcDirectory("");
			if (!fileSystem.ExistsDirectory(dst)) op.CreateDirectory("");

			var srcDirs = Task.Run(fileSystem.EnumerateDirsRecursive(src)
				.Ignore(options.IgnoreDirectories).ToArray, token);
			var dstDirs = Task.Run(fileSystem.EnumerateDirsRecursive(dst)
				.Ignore(options.IgnoreDirectories).ToArray, token);

			srcDirsRelative = srcDirs.Result.ToRelative(src).ToHashSet();
			dstDirsRelative = dstDirs.Result.ToRelative(dst).ToHashSet();

			srcFilesRelative = Task.Run(()
				=> fileSystem.EnumerateFiles(srcDirs.Result).ToRelative(src)
				.Ignore(options.IgnoreFiles).ToHashSet(), token);
			dstFilesRelative = Task.Run(()
				=> fileSystem.EnumerateFiles(dstDirs.Result).ToRelative(dst)
				.Ignore(options.IgnoreFiles).ToHashSet(), token);
		}
	}
}
