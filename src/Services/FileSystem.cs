using System;
using System.IO;
using VersionedCopy.Interfaces;

namespace VersionedCopy.Services
{
	internal class FileSystem : IFileSystem
	{
		public FileSystem(ILogger logger, bool logErrors)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			LogErrors = logErrors;
		}

		public ILogger Logger { get; }
		public bool LogErrors { get; }

		public void Copy(string srcFilePath, string dstFilePath)
		{
			LogCatch(() =>
			{
				File.Copy(srcFilePath, dstFilePath);
			});
		}

		public void CreateDirectory(string path) => LogCatch(() => Directory.CreateDirectory(path));

		public void MoveDirectory(string source, string destination)
		{
			LogCatch(() =>
			{
				Directory.CreateDirectory(destination + "..");
				Directory.Move(source, destination);
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