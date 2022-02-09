using System.Collections.Generic;

namespace VersionedCopy.Interfaces
{
	public interface IFileSystem
	{
		bool Copy(string srcFilePath, string dstFilePath);
		int CompareAge(string source, string destination);
		bool CreateDirectory(string path);
		IEnumerable<string> EnumerateDirsRecursive(string dir);
		IEnumerable<string> EnumerateFiles(IEnumerable<string> dirs);
		bool ExistsDirectory(string name);
		bool ExistsFile(string name);
		bool HasChanged(string srcFilePath, string dstFilePath);
		bool MoveDirectory(string source, string destination);
		bool MoveFile(string source, string destination);
	}
}