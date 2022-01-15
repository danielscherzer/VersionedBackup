using System.Collections.Generic;

namespace VersionedBackup.Interfaces
{
	internal interface IFileSystem
	{
		bool CreateDirectory(string path);
		bool Copy(string srcFilePath, string dstFilePath);
		bool ExistsDirectory(string name);
		bool ExistsFile(string name);
		bool HasChanged(string srcFilePath, string dstFilePath);
		bool MoveDirectory(string source, string destination);
		bool MoveFile(string source, string destination);
	}
}