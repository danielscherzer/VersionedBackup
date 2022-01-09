using System;
using System.IO;
using VersionedBackup.Interfaces;

namespace VersionedBackup.Services
{
	internal class FileOperation : IDisposable
	{
		private readonly ILogger logger;
		private readonly IOptions options;
		private readonly IFileSystem fileSystem;
		private readonly Report report;

		public FileOperation(ILogger logger, IOptions options)
		{
			this.logger = logger;
			this.options = options;
			// create file sysem service
			fileSystem = options.DryRun ? new NullFileSystem() : new FileSystem(logger, options.LogErrors);
			report = new();
		}

		internal void CreateDirectory(string directory) => fileSystem.CreateDirectory(directory);

		internal void MoveAwayOldDir(string subDir)
		{
			string source = options.DestinationDirectory + subDir;
			if (Directory.Exists(source))
			{
				string destination = options.OldFilesFolder + subDir;
				LogOperation($"Moving old directory '{subDir}' to {options.OldFilesFolder}");
				fileSystem.MoveDirectory(source, destination);
				Report($"Old directory '{subDir}'");
			}

		}

		internal void MoveAwayDeleted(string fileName)
		{
			string moveAwayFileName = options.DestinationDirectory + fileName;
			if (File.Exists(moveAwayFileName))
			{
				string destination = options.OldFilesFolder + fileName;
				LogOperation($"Moving deleted file '{fileName}' to '{options.OldFilesFolder}'");
				fileSystem.MoveFile(moveAwayFileName, destination);
				Report($"Deleted file '{fileName}'");
			}
		}

		internal void CopyNewFile(string fileName)
		{
			LogOperation($"Copy new file '{fileName}' to '{options.DestinationDirectory}'");
			var srcFilePath = options.SourceDirectory + fileName;
			var dstFilePath = options.DestinationDirectory + fileName;
			fileSystem.Copy(srcFilePath, dstFilePath);
		}

		internal void UpdateFile(string fileName)
		{
			var srcFilePath = options.SourceDirectory + fileName;
			var dstFilePath = options.DestinationDirectory + fileName;
			var srcFileInfo = GetFileInfo(srcFilePath);
			if (srcFileInfo is null) return;
			var dstFileInfo = GetFileInfo(dstFilePath);
			if (dstFileInfo is null) return;
			TimeSpan writeDiff = srcFileInfo.LastWriteTimeUtc.Subtract(dstFileInfo.LastWriteTimeUtc);
			if (Math.Abs(writeDiff.TotalSeconds) > 5 || srcFileInfo.Length != dstFileInfo.Length)
			{
				// move old to oldFilesFolder
				LogOperation($"Moving old version of file '{fileName}' to '{options.OldFilesFolder}'");
				fileSystem.MoveFile(dstFilePath, options.OldFilesFolder + fileName);
				Report($"Old verion of file '{fileName}'");
				// copy new to dst
				LogOperation($"Copy new version of file '{fileName}' to '{options.DestinationDirectory}'");
				fileSystem.Copy(srcFilePath, dstFilePath);
			}
		}

		private FileInfo? GetFileInfo(string path)
		{
			try
			{
				return new FileInfo(path);
			}
			catch (SystemException e)
			{
				if (options.LogErrors) logger.Log(e.Message);
				return null;
			}
		}

		private void LogOperation(string message)
		{
			if (options.LogOperations) logger.Log(message);
		}

		private void Report(string message)
		{
			if (!options.DryRun) report.Add(message);
		}

		public void Dispose()
		{
			report.Save(options.OldFilesFolder + "report.txt");
		}
	}
}
