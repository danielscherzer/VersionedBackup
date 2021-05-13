using System;
using System.IO;
using VersionedCopy.Interfaces;

namespace VersionedCopy.Services
{
	internal class FileSystem : IFileSystem
	{
		public FileSystem(ILogger logger, bool logOperations, bool logErrors)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			LogOperations = logOperations;
			LogErrors = logErrors;
		}

		public ILogger Logger { get; }
		public bool LogOperations { get; }
		public bool LogErrors { get; }

		public void Copy(string srcFilePath, string dstFilePath)
		{
			LogCatch(() =>
			{
				File.Copy(srcFilePath, dstFilePath);
				if (LogOperations) Logger.Log($"Copy file '{srcFilePath}' => '{dstFilePath}'");
			});
		}

		public void CreateDirectory(string path) => LogCatch(() => Directory.CreateDirectory(path));

		public FileInfo? GetFileInfo(string path)
		{
			try
			{
				return new FileInfo(path);
			}
			catch (SystemException e)
			{
				if (LogErrors) Logger.Log(e.Message);
				return null;
			}
		}

		public void MoveDirectory(string source, string destination)
		{
			LogCatch(() =>
			{
				Directory.CreateDirectory(destination + "..");
				Directory.Move(source, destination);
				if (LogOperations) Logger.Log($"Moving directory '{source}' => '{destination}'");
			});
		}

		public void MoveFile(string source, string destination)
		{
			LogCatch(() =>
			{
				var parentDir = Path.GetDirectoryName(destination);
				if(parentDir is null)
				{
					if(LogErrors) Logger.Log($"File '{destination}' has no parent directory.");
					return;
				}
				Directory.CreateDirectory(parentDir);
				File.Move(source, destination);
				if (LogOperations) Logger.Log($"Moving file '{source}' => '{destination}'");
			});
		}

		private void LogCatch(Action action)
		{
			try
			{
				action();
			}
			catch (IOException e)
			{
				if (LogErrors) Logger.Log(e.Message);
			}
		}
	}
}