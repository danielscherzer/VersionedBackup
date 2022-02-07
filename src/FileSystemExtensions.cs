using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy
{
	internal static class FileSystemExtensions
	{
		public static Task<string[]> EnumerateDirsAsync(this IFileSystem fileSystem, string directory, IEnumerable<string> ignoreDirectories, CancellationToken token)
		{
			return Task.Run(fileSystem.EnumerateDirsRecursive(directory).Ignore(ignoreDirectories).ToArray, token);
		}
	}
}
