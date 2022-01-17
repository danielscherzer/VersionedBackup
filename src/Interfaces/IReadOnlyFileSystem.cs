using System.Collections.Generic;

namespace VersionedCopy.Interfaces
{
	public interface IReadOnlyFileSystem
	{
		IEnumerable<string> EnumerateDirsRecursive(string dir);
		IEnumerable<string> EnumerateFiles(IEnumerable<string> dirs);
		bool ExistsDirectory(string name);
		bool ExistsFile(string name);
		bool HasChanged(string srcFilePath, string dstFilePath);
	}
}