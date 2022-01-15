using VersionedBackup.Interfaces;

namespace VersionedBackup.Services
{
	public class FileSystemOperations
	{
		private readonly IDirectories directories;
		private readonly IFileSystem fileSystem;
		private readonly IReport report;

		public FileSystemOperations(IReport report, IDirectories directories, IFileSystem fileSystem)
		{
			this.report = report;
			this.directories = directories;
			this.fileSystem = fileSystem;
		}

		internal void CreateDirectory(string subDir)
		{
			string directory = directories.DestinationDirectory + subDir;
			if (fileSystem.CreateDirectory(directory))
			{
				report.Add(Operation.CreateDir, subDir);
			}
		}

		internal void MoveAwayDeletedDir(string subDir)
		{
			string source = directories.DestinationDirectory + subDir;
			if (fileSystem.ExistsDirectory(source))
			{
				string destination = directories.OldFilesFolder + subDir;
				if (fileSystem.MoveDirectory(source, destination))
				{
					report.Add(Operation.DeleteDir, subDir);
				}
			}
		}

		internal void MoveAwayDeleted(string fileName)
		{
			string moveAwayFileName = directories.DestinationDirectory + fileName;
			if (fileSystem.ExistsFile(moveAwayFileName))
			{
				string destination = directories.OldFilesFolder + fileName;
				if (fileSystem.MoveFile(moveAwayFileName, destination))
				{
					report.Add(Operation.DeleteFile, fileName);
				}
			}
		}

		internal void CopyNewFile(string fileName)
		{
			var srcFilePath = directories.SourceDirectory + fileName;
			var dstFilePath = directories.DestinationDirectory + fileName;
			if (fileSystem.Copy(srcFilePath, dstFilePath))
			{
				report.Add(Operation.NewFile, fileName);
			}
		}

		internal void UpdateFile(string fileName)
		{
			var srcFilePath = directories.SourceDirectory + fileName;
			var dstFilePath = directories.DestinationDirectory + fileName;
			if (fileSystem.HasChanged(srcFilePath, dstFilePath))
			{
				// move old to oldFilesFolder
				if (fileSystem.MoveFile(dstFilePath, directories.OldFilesFolder + fileName))
				{
					// copy new to dst
					if (fileSystem.Copy(srcFilePath, dstFilePath))
					{
						report.Add(Operation.UpdateFile, fileName);
					}
				}
			}
		}
	}
}
