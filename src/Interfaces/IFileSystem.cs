namespace VersionedCopy.Interfaces
{
	public interface IFileSystem : IReadOnlyFileSystem
	{
		bool CreateDirectory(string path);
		bool Copy(string srcFilePath, string dstFilePath);
		bool MoveDirectory(string source, string destination);
		bool MoveFile(string source, string destination);
		bool IsNewer(string source, string destination);
	}
}