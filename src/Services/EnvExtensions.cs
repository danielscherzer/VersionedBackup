using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services
{
	using Entry = KeyValuePair<string, DateTime>;

	internal static class EnvExtensions
	{
		public static void Copy(this Env env, RelativeFileList toCopy, Snapshot snapDst)
		{
			var list = toCopy.Items is IList<Entry> ? toCopy.Items : toCopy.ToList();
			foreach (var file in list.Where(file => !Snapshot.IsFile(file.Key)))
			{
				var fileName = file.Key;
				if (env.Token.IsCancellationRequested) return;
				if (env.FileSystem.CreateDirectory(snapDst.FullName(fileName), file.Value))
				{
					snapDst.Entries[fileName] = file.Value;
					env.Output.Report($"Create directory '{snapDst.FullName(fileName)}'");
				}
			}
			foreach (var file in list.Where(file => Snapshot.IsFile(file.Key)))
			{
				var fileName = file.Key;
				if (env.Token.IsCancellationRequested) return;
				if (env.FileSystem.Copy(toCopy.FullName(fileName), snapDst.FullName(fileName)))
				{
					snapDst.Entries[fileName] = file.Value;
					env.Output.Report($"New file '{snapDst.FullName(fileName)}'");
				}
			}
		}

		public static void MoveAway(this Env env, Snapshot snapshot, IEnumerable<Entry> toDelete)
		{
			var list = toDelete is IList<Entry> ? toDelete : toDelete.ToList();
			List<string> alreadyMovedDirs = new();
			foreach (var file in list)
			{
				if (env.Token.IsCancellationRequested) return;
				var fileName = file.Key;
				// do not need to move directory/file where parent dir has been moved
				if (alreadyMovedDirs.Any(entry => fileName.Contains(entry)))
				{
					snapshot.Entries.Remove(fileName);
					continue;
				}
				var path = snapshot.FullName(fileName);
				//move deleted to old
				if (Snapshot.IsFile(fileName))
				{
					if (env.FileSystem.MoveFile(path, snapshot.BackupName(file)))
					{
						snapshot.Entries.Remove(fileName);
						env.Output.Report($"Backup deleted file '{path}'");
					}
				}
				else
				{
					if (env.FileSystem.MoveDirectory(path, snapshot.BackupName(file)))
					{
						snapshot.Entries.Remove(fileName);
						alreadyMovedDirs.Add(fileName);
						env.Output.Report($"Backup deleted directory '{path}'");
					}
				}
			}
		}

		public static void UpdateFiles(this Env env, RelativeFileList updatedFiles, Snapshot snapDst)
		{
			foreach (var file in updatedFiles)
			{
				if (env.Token.IsCancellationRequested) return;
				var fileName = file.Key;
				var dstPath = snapDst.FullName(fileName);
				if (env.FileSystem.MoveFile(dstPath, snapDst.BackupName(file))) //move away old
				{
					if (env.FileSystem.Copy(updatedFiles.FullName(fileName), dstPath)) //copy new
					{
						snapDst.Entries[fileName] = file.Value;
						env.Output.Report($"Update file '{dstPath}'");
					}
				}
			}
		}

		public static void SetTimeStamp(this Env env, string fileName, DateTime newTime)
		{
			env.Output.Report($"New time stamp '{fileName}'");
			env.FileSystem.SetTimeStamp(fileName, newTime);
		}

		public static bool Setup(string sourceDirectory, string destinationDirectory, IOutput output, bool readOnly, FileSystem fileSystem)
		{
			if (sourceDirectory == destinationDirectory)
			{
				output.Error("Source and destination must be different!");
				return false;
			}
			if (!Directory.Exists(sourceDirectory))
			{
				output.Error($"Source directory '{sourceDirectory}' does not exist");
				return false;
			}
			if (!Directory.Exists(destinationDirectory))
			{
				if (readOnly)
				{
					output.Error($"Destination directory '{destinationDirectory}' does not exist");
					return false;
				}
				else
				{
					if (fileSystem.CreateDirectory(destinationDirectory))
					{
						output.Report($"Create directory '{destinationDirectory}'");
					}
				}
			}
			if (readOnly) output.Report("Read only mode");
			return true;
		}

		//TODO: multithreaded does not work well with stick internal void Copy(string src, string dst, IEnumerable<Entry> srcNew)
		//{
		//	try
		//	{
		//		// create directories
		//		Parallel.ForEach(srcNew.Where(file => !Snapshot.IsFile(file.Key)), new ParallelOptions { CancellationToken = Token }, file =>
		//		{
		//			var fileName = file.Key;
		//			if (FileOperations.CreateDirectory(dst + fileName, file.Value))
		//			{
		//				Output.Report($"Create directory '{dst + fileName}'");
		//			}
		//		});
		//		// files
		//		Parallel.ForEach(srcNew.Where(file => Snapshot.IsFile(file.Key)), new ParallelOptions { CancellationToken = Token }, file =>
		//		{
		//			var fileName = file.Key;
		//			if (FileOperations.Copy(src + fileName, dst + fileName))
		//			{
		//				Output.Report($"New file '{dst + fileName}'");
		//			}
		//		});
		//	}
		//	catch (OperationCanceledException)
		//	{
		//	}
		//}

		//TODO: multithreaded does not work well with stick internal void UpdateFiles(string src, string dst, IEnumerable<Entry> updatedFiles)
		//{
		//	try
		//	{
		//		Parallel.ForEach(updatedFiles, new ParallelOptions { CancellationToken = Token }, file =>
		//		{
		//			var fileName = file.Key;
		//			if (FileOperations.MoveFile(dst + fileName, Options.OldFilesFolder + fileName)) //move away old
		//			{
		//				if (FileOperations.Copy(src + fileName, dst + fileName)) //copy new
		//				{
		//					Output.Report($"Update file '{dst + fileName}'");
		//				}
		//			}
		//		});
		//	}
		//	catch (OperationCanceledException)
		//	{
		//	}
		//}
	}
}
