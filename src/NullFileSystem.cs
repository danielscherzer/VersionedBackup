using VersionedCopy.Interfaces;

internal class NullFileSystem : IFileSystem
{
	public void Copy(string srcFilePath, string dstFilePath)
	{
	}

	public void CreateDirectory(string path)
	{
	}

	public void MoveDirectory(string source, string destination)
	{
	}

	public void MoveFile(string source, string destination)
	{
	}
}