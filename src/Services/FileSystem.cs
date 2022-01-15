using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VersionedBackup.Interfaces;

namespace VersionedBackup.Services
{
	internal class FileSystem : IFileSystem
	{
		public FileSystem(ILogger logger, bool readOnly)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			ReadOnly = readOnly;
		}

		private ILogger Logger { get; }
		private bool ReadOnly { get; }

		public bool CreateDirectory(string path)
		{
			if (ReadOnly) return true;
			return Successful(() => Directory.CreateDirectory(path));
		}

		public bool Copy(string srcFilePath, string dstFilePath)
		{
			if (ReadOnly) return true;
			return Successful(() =>
			{
				File.Copy(srcFilePath, dstFilePath);
			});
		}

		//TODO: make multi threaded
		public IEnumerable<string> EnumerateDirsRecursive(string dir)
		{
			var stack = new Stack<string>();
			if (!Directory.Exists(dir)) yield break;
			stack.Push(dir);
			yield return dir;
			while (stack.Count > 0)
			{
				foreach (var subDir in Directory.EnumerateDirectories(stack.Pop()))
				{
					yield return subDir + Path.DirectorySeparatorChar;
					stack.Push(subDir);
				}
			}
		}

		public IEnumerable<string> EnumerateFiles(IEnumerable<string> dirs) =>
			from subDir in dirs.AsParallel()
			from file in Directory.EnumerateFiles(subDir)
			select file;

		public bool ExistsDirectory(string name) => Directory.Exists(name);

		public bool ExistsFile(string name) => File.Exists(name);

		public bool HasChanged(string source, string destination)
		{
			try
			{
				var srcFileInfo = new FileInfo(source);
				var dstFileInfo = new FileInfo(destination);
				TimeSpan writeDiff = srcFileInfo.LastWriteTimeUtc.Subtract(dstFileInfo.LastWriteTimeUtc);
				return (Math.Abs(writeDiff.TotalSeconds) > 5 || srcFileInfo.Length != dstFileInfo.Length);
			}
			catch (SystemException e)
			{
				Logger.Add($"ERROR: {e.Message}");
				return false;
			}
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
					Logger.Add($"ERROR: File '{destination}' has no parent directory.");
					return;
				}
				Directory.CreateDirectory(parentDir);
				File.Move(source, destination);
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
				Logger.Add($"ERROR: {e.Message}");
				return false;
			}
		}
	}
}