using VersionedCopy.Interfaces;

namespace VersionedCopy.Services
{
	public class Operations
	{
		private readonly IFileSystem fileSystem;
		private readonly IOutput output;
		private readonly string src;
		private readonly string dst;
		private readonly string old;

		public Operations(IOutput output, IDirectories directories, IFileSystem fileSystem)
		{
			this.output = output;
			this.fileSystem = fileSystem;
			src = directories.SourceDirectory;
			dst = directories.DestinationDirectory;
			old = directories.OldFilesFolder;
		}

		internal void CopyNewFile(string fileName)
		{
			var srcFilePath = src + fileName;
			var dstFilePath = dst + fileName;
			if (fileSystem.Copy(srcFilePath, dstFilePath))
			{
				output.Report($"New file '{fileName}'");
			}
		}

		internal void CopyUpdatedFile(string fileName)
		{
			var srcFilePath = src + fileName;
			var dstFilePath = dst + fileName;
			if (0 < fileSystem.CompareAge(srcFilePath, dstFilePath))
			{
				// move old to oldFilesFolder
				if (fileSystem.MoveFile(dstFilePath, old + fileName))
				{
					// copy updated to dst
					if (fileSystem.Copy(srcFilePath, dstFilePath))
					{
						output.Report($"Replace file '{fileName}'");
					}
				}
			}
		}

		internal void CreateDirectory(string subDir)
		{
			string directory = dst + subDir;
			if (fileSystem.CreateDirectory(directory))
			{
				output.Report($"Create directory '{directory}'");
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
					output.Report($"Delete directory '{subDir}'");
				}
			}
		}

		internal void MoveAwayDeletedFile(string fileName)
		{
			string moveAwayFileName = dst + fileName;
			if (fileSystem.ExistsFile(moveAwayFileName))
			{
				string destination = old + fileName;
				if (fileSystem.MoveFile(moveAwayFileName, destination))
				{
					output.Report($"Backup file '{fileName}'");
				}
			}
		}

		internal void ReplaceChangedFile(string fileName)
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
						output.Report($"Replace file '{fileName}'");
					}
				}
			}
		}
	}
}

