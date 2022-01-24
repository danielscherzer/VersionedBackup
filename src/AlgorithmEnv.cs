using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public class AlgorithmEnv
	{
		/// <summary>
		/// optimize common update case: assume two directory structures are very similar
		/// 1=> read multi-threaded all directories and then all files
		/// 2=> optimize compare of almost all files for date time and size
		/// </summary>
		/// <param name="options"></param>
		/// <param name="token"><see cref="CancellationToken"/></param>
		public AlgorithmEnv(IOptions options, IReport report, IFileSystem fileSystem, CancellationToken token)
		{
			Op = new Operations(report, options, fileSystem);
			Options = options;
			FileSystem = fileSystem;
			Token = token;
		}

		public Task<string[]> EnumerateDirsAsync(string directory)
		{
			return Task.Run(FileSystem.EnumerateDirsRecursive(directory)
				.Ignore(Options.IgnoreDirectories).ToArray, Token);
		}

		public Task<HashSet<string>> EnumerateRelativeFilesAsync(string root, IEnumerable<string> directories)
		{
			root = root.IncludeTrailingPathDelimiter();
			return Task.Run(()
				=> FileSystem.EnumerateFiles(directories).ToRelative(root)
				.Ignore(Options.IgnoreFiles).ToHashSet(), Token);
		}

		public IOptions Options { get; }
		public IFileSystem FileSystem { get; }
		public CancellationToken Token { get; }
		public Operations Op { get; }
	}
}
