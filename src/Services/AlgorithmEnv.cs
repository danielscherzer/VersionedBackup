using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services
{
	using Entry = KeyValuePair<string, DateTime>;

	public class AlgorithmEnv
	{
		public AlgorithmEnv(IOptions options, IOutput output, CancellationToken token)
		{
			Options = options;
			Output = output;
			FileOperations = new FileOperations(output, options.ReadOnly);
			Token = token;
			output.Report("VersionedCopy");
			if (options.ReadOnly) output.Report("Read only mode");
			output.Report($"Ignore directories: { string.Join(';', options.IgnoreDirectories)}");
			output.Report($"Ignore files: { string.Join(';', options.IgnoreFiles)}");
			if (options.SourceDirectory == options.DestinationDirectory)
			{
				output.Error("Source and destination must be different!");
				return;

			}
			if (!Directory.Exists(options.SourceDirectory))
			{
				output.Error($"Source directory '{options.SourceDirectory}' does not exist");
				return;
			}
			if (!Directory.Exists(options.DestinationDirectory))
			{
				if (options.ReadOnly)
				{
					output.Error($"Destination directory '{options.DestinationDirectory}' does not exist");
					return;
				}
				else
				{
					if (FileOperations.CreateDirectory(options.DestinationDirectory))
					{
						output.Report($"Create directory '{options.DestinationDirectory}'");
					}
				}
			}
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

		internal void Copy(RelativeFileList toCopy, Snapshot snapDst)
		{
			var list = toCopy.Items is IList<Entry> ? toCopy.Items : toCopy.ToList();
			foreach (var file in list.Where(file => !Snapshot.IsFile(file.Key)))
			{
				var fileName = file.Key;
				if (Token.IsCancellationRequested) return;
				if (FileOperations.CreateDirectory(snapDst.FullName(fileName), file.Value))
				{
					snapDst.Entries[fileName] = file.Value;
					Output.Report($"Create directory '{snapDst.FullName(fileName)}'");
				}
			}
			foreach (var file in list.Where(file => Snapshot.IsFile(file.Key)))
			{
				var fileName = file.Key;
				if (Token.IsCancellationRequested) return;
				if (FileOperations.Copy(toCopy.FullName(fileName), snapDst.FullName(fileName)))
				{
					snapDst.Entries[fileName] = file.Value;
					Output.Report($"New file '{snapDst.FullName(fileName)}'");
				}
			}
		}

		internal Snapshot CreateSnapshot(string root)
		{
			return Snapshot.Create(root, Options.IgnoreDirectories, Options.IgnoreFiles, Token);
		}

		internal void MoveAway(Snapshot snapshot, IEnumerable<Entry> toDelete)
		{
			var list = toDelete is IList<Entry> ? toDelete : toDelete.ToList();
			List<string> alreadyMovedDirs = new();
			foreach (var file in list)
			{
				if (Token.IsCancellationRequested) return;
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
					if (FileOperations.MoveFile(path, snapshot.BackupDir + fileName))
					{
						snapshot.Entries.Remove(fileName);
						Output.Report($"Backup deleted file '{path}'");
					}
				}
				else
				{
					if (FileOperations.MoveDirectory(path, snapshot.BackupDir + fileName))
					{
						snapshot.Entries.Remove(fileName);
						alreadyMovedDirs.Add(fileName);
						Output.Report($"Backup deleted directory '{path}'");
					}
				}
			}
		}

		internal void SetTimeStamp(string fileName, DateTime newTime)
		{
			Output.Report($"New time stamp '{fileName}'");
			FileOperations.SetTimeStamp(fileName, newTime);
		}

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

		internal void UpdateFiles(RelativeFileList updatedFiles, Snapshot snapDst)
		{
			foreach (var file in updatedFiles)
			{
				if (Token.IsCancellationRequested) return;
				var fileName = file.Key;
				var dstPath = snapDst.FullName(fileName);
				if (FileOperations.MoveFile(dstPath, snapDst.BackupDir + fileName)) //move away old
				{
					if (FileOperations.Copy(updatedFiles.FullName(fileName), dstPath)) //copy new
					{
						snapDst.Entries[fileName] = file.Value;
						Output.Report($"Update file '{dstPath}'");
					}
				}
			}
		}

		public bool Canceled => Token.IsCancellationRequested;
		public IOptions Options { get; }
		private CancellationToken Token { get; }
		private FileOperations FileOperations { get; }
		private IOutput Output { get; }
	}
}
