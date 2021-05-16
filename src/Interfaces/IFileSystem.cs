namespace VersionedCopy.Interfaces
{
	internal interface IFileSystem
	{
		void Copy(string srcFilePath, string dstFilePath);
		void CreateDirectory(string path);
		void MoveDirectory(string source, string destination);
		void MoveFile(string source, string destination);
	}
}