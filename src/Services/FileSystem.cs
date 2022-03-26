using System;
using System.IO;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services
{
	public class FileSystem
	{
		public FileSystem(IOutput output, bool readOnly)
		{
			Output = output ?? throw new ArgumentNullException(nameof(output));
			ReadOnly = readOnly;
		}

		private IOutput Output { get; }
		private bool ReadOnly { get; }

		public bool CreateDirectory(string path)
		{
			if (ReadOnly) return true;
			return Successful(() => Directory.CreateDirectory(path));
		}

		public bool CreateDirectory(string path, DateTime creationTimeUtc)
		{
			if (ReadOnly) return true;
			return Successful(() => Directory.CreateDirectory(path).CreationTimeUtc = creationTimeUtc);
		}

		public bool Copy(string srcFilePath, string dstFilePath)
		{
			if (ReadOnly) return true;
			return Successful(() =>
			{
				File.Copy(srcFilePath, dstFilePath);
			});
		}

		public bool MoveDirectory(string source, string destination)
		{
			if (ReadOnly) return true;
			return Successful(() =>
			{
				Directory.CreateDirectory(destination + "..");
				Directory.Move(source, destination);
			});
		}

		public bool MoveFile(string source, string destination)
		{
			if (ReadOnly) return true;
			return Successful(() =>
			{
				var parentDir = Path.GetDirectoryName(destination);
				if (parentDir is null)
				{
					Output.Error($"File '{destination}' has no parent directory.");
					return;
				}
				Directory.CreateDirectory(parentDir);
				File.Move(source, destination);
			});
		}

		public bool SetTimeStamp(string filePath, DateTime newTime)
		{
			if (ReadOnly) return true;
			return Successful(() =>
			{
				if (Snapshot.IsFile(filePath))
				{
					var file = new FileInfo(filePath);
					var wasReadOnly = file.IsReadOnly;
					if(wasReadOnly) file.IsReadOnly = false;
					file.LastWriteTimeUtc = newTime;
					if(wasReadOnly) file.IsReadOnly = true;
				}
				else
				{
					var dir = new DirectoryInfo(filePath);
					var wasReadOnly = 0 != (dir.Attributes & FileAttributes.ReadOnly);
					if (wasReadOnly) dir.Attributes &= ~FileAttributes.ReadOnly;
					dir.CreationTimeUtc = newTime;
					if(wasReadOnly) dir.Attributes |= FileAttributes.ReadOnly;
				}
			});
		}

		private bool Successful(Action action)
		{
			try
			{
				action();
				return true;
			}
			catch (IOException e)
			{
				Output.Error(e.Message);
				return false;
			}
		}
	}
}