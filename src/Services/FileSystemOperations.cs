using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services
{
	public class FileSystemOperations
	{
		private readonly IFileSystem fileSystem;
		private readonly IReport report;
		private readonly string src;
		private readonly string dst;
		private readonly string old;

		public FileSystemOperations(IReport report, IDirectories directories, IFileSystem fileSystem)
		{
			this.report = report;
			this.fileSystem = fileSystem;
			src = directories.SourceDirectory.IncludeTrailingPathDelimiter();
			dst = directories.DestinationDirectory.IncludeTrailingPathDelimiter();
			old = directories.OldFilesFolder.IncludeTrailingPathDelimiter();
		}

		internal void CreateDirectory(string subDir)
		{
			string directory = dst + subDir;
			if (fileSystem.CreateDirectory(directory))
			{
				report.Add(Operation.CreateDir, subDir);
			}
		}

		internal void MoveAwayDeletedDir(string subDir)
		{
			string source = dst + subDir;
			if (fileSystem.ExistsDirectory(source))
			{
				string destination = old + subDir;
				if (fileSystem.MoveDirectory(source, destination))
				{
					report.Add(Operation.DeleteDir, subDir);
				}
			}
		}

		internal void MoveAwayDeleted(string fileName)
		{
			string moveAwayFileName = dst + fileName;
			if (fileSystem.ExistsFile(moveAwayFileName))
			{
				string destination = old + fileName;
				if (fileSystem.MoveFile(moveAwayFileName, destination))
				{
					report.Add(Operation.DeleteFile, fileName);
				}
			}
		}

		internal void CopyNewFile(string fileName)
		{
			var srcFilePath = src + fileName;
			var dstFilePath = dst + fileName;
			if (fileSystem.Copy(srcFilePath, dstFilePath))
			{
				report.Add(Operation.NewFile, fileName);
			}
		}

		internal void CopyChangedFile(string fileName)
		{
			var srcFilePath = src + fileName;
			var dstFilePath = dst + fileName;
			if (fileSystem.HasChanged(srcFilePath, dstFilePath))
			{
				// move old to oldFilesFolder
				if (fileSystem.MoveFile(dstFilePath, old + fileName))
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
