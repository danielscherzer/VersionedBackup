using System.Collections.Generic;
using VersionedBackup.Interfaces;

namespace VersionedBackupTests.Services
{
	internal class ListFileSystem : IFileSystem
	{
		public bool Copy(string srcFilePath, string dstFilePath)
		{
			return true;
		}

		public bool CreateDirectory(string path)
		{
			return true;
		}

		public IEnumerable<string> EnumerateDirsRecursive(string dir)
		{
			yield return dir;
		}

		public IEnumerable<string> EnumerateFiles(IEnumerable<string> dirs)
		{
			yield break;
		}

		public bool ExistsDirectory(string name)
		{
			return true;
		}

		public bool ExistsFile(string name)
		{
			return true;
		}

		public bool HasChanged(string srcFilePath, string dstFilePath)
		{
			return false;
		}

		public bool MoveDirectory(string source, string destination)
		{
			return true;
		}

		public bool MoveFile(string source, string destination)
		{
			return true;
		}
	}
}