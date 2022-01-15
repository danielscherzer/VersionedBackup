using VersionedBackup.Interfaces;

namespace VersionedBackup.Services
{
	internal class FileOperation
	{
		private readonly IOperation operationData;
		private readonly IFileSystem fileSystem;
		private readonly Report report;

		public FileOperation(Report report, IOperation operationData, IFileSystem fileSystem)
		{
			this.report = report;
			this.operationData = operationData;
			this.fileSystem = fileSystem;
		}

		internal void CreateDirectory(string subDir)
		{
			string directory = operationData.DestinationDirectory + subDir;
			if (fileSystem.CreateDirectory(directory))
			{
				report.Add($"Create directory '{subDir}'");
			}
		}

		internal void MoveAwayDeletedDir(string subDir)
		{
			string source = operationData.DestinationDirectory + subDir;
			if (fileSystem.ExistsDirectory(source))
			{
				string destination = operationData.OldFilesFolder + subDir;
				if(fileSystem.MoveDirectory(source, destination))
				{
					report.Add($"Deleted directory '{subDir}'");
				}
			}
		}

		internal void MoveAwayDeleted(string fileName)
		{
			string moveAwayFileName = operationData.DestinationDirectory + fileName;
			if (fileSystem.ExistsFile(moveAwayFileName))
			{
				string destination = operationData.OldFilesFolder + fileName;
				if(fileSystem.MoveFile(moveAwayFileName, destination))
				{
					report.Add($"Deleted file '{fileName}'");
				}
			}
		}

		internal void CopyNewFile(string fileName)
		{
			var srcFilePath = operationData.SourceDirectory + fileName;
			var dstFilePath = operationData.DestinationDirectory + fileName;
			if(fileSystem.Copy(srcFilePath, dstFilePath))
			{
				report.Add($"New file '{fileName}'");
			}
		}

		internal void UpdateFile(string fileName)
		{
			var srcFilePath = operationData.SourceDirectory + fileName;
			var dstFilePath = operationData.DestinationDirectory + fileName;
			if(fileSystem.HasChanged(srcFilePath, dstFilePath))
			{
				// move old to oldFilesFolder
				if(fileSystem.MoveFile(dstFilePath, operationData.OldFilesFolder + fileName))
				{
					// copy new to dst
					if(fileSystem.Copy(srcFilePath, dstFilePath))
					{
						report.Add($"Update file '{fileName}'");
					}
				}
			}
		}
	}
}
