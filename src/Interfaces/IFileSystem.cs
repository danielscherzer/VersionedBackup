namespace VersionedCopy.Interfaces
{
	public interface IFileSystem
	{
		bool Copy(string srcFilePath, string dstFilePath);
		bool CreateDirectory(string path);
		bool MoveDirectory(string source, string destination);
		bool MoveFile(string source, string destination);
	}
}