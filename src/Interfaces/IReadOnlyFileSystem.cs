namespace VersionedBackup.Interfaces
{
	internal interface IReadOnlyFileSystem
	{
		bool ExistsDirectory(string name);
		bool ExistsFile(string name);
		bool HasChanged(string srcFilePath, string dstFilePath);
	}
}