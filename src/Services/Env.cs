using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VersionedCopy.Options;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services
{
	using Entry = KeyValuePair<string, DateTime>;

	public class Env
	{
		public Env(DiffOptions options, Output output, CancellationToken token)
		{
			output.Report("VersionedCopy");
			if (options.ReadOnly) output.Report("Read only mode");
			output.Report($"Ignore directories: { string.Join(';', options.IgnoreDirectories)}");
			output.Report($"Ignore files: { string.Join(';', options.IgnoreFiles)}");
			FileSystem = new FileSystem(output, options.ReadOnly);
			IgnoreDirectories = options.IgnoreDirectories.Select(dir => dir.NormalizePathDelimiter().IncludeTrailingPathDelimiter());
			IgnoreFiles = options.IgnoreFiles.Select(file => file.NormalizePathDelimiter());
			Output = output;
			Token = token;
		}

		public FileSystem FileSystem { get; }
		public IEnumerable<string> IgnoreDirectories { get; }
		public IEnumerable<string> IgnoreFiles { get; }
		public Output Output { get; }
		public CancellationToken Token { get; }

		internal void Copy(RelativeFileList toCopy, Snapshot snapDst)
		{
			var list = toCopy.Items is IList<Entry> ? toCopy.Items : toCopy.ToList();
			foreach (var file in list.Where(file => !Snapshot.IsFile(file.Key)))
			{
				var fileName = file.Key;
				if (Token.IsCancellationRequested) return;
				if (FileSystem.CreateDirectory(snapDst.FullName(fileName), file.Value))
				{
					snapDst.Entries[fileName] = file.Value;
					Output.Report($"Create directory '{snapDst.FullName(fileName)}'");
				}
			}
			foreach (var file in list.Where(file => Snapshot.IsFile(file.Key)))
			{
				var fileName = file.Key;
				if (Token.IsCancellationRequested) return;
				if (FileSystem.Copy(toCopy.FullName(fileName), snapDst.FullName(fileName)))
				{
					snapDst.Entries[fileName] = file.Value;
					Output.Report($"New file '{snapDst.FullName(fileName)}'");
				}
			}
		}

		internal Snapshot CreateSnapshot(string root)
		{
			return Snapshot.Create(root, IgnoreDirectories, IgnoreFiles, Token);
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
					if (FileSystem.MoveFile(path, snapshot.BackupDir + fileName))
					{
						snapshot.Entries.Remove(fileName);
						Output.Report($"Backup deleted file '{path}'");
					}
				}
				else
				{
					if (FileSystem.MoveDirectory(path, snapshot.BackupDir + fileName))
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
			FileSystem.SetTimeStamp(fileName, newTime);
		}

		internal void UpdateFiles(RelativeFileList updatedFiles, Snapshot snapDst)
		{
			foreach (var file in updatedFiles)
			{
				if (Token.IsCancellationRequested) return;
				var fileName = file.Key;
				var dstPath = snapDst.FullName(fileName);
				if (FileSystem.MoveFile(dstPath, snapDst.BackupDir + fileName)) //move away old
				{
					if (FileSystem.Copy(updatedFiles.FullName(fileName), dstPath)) //copy new
					{
						snapDst.Entries[fileName] = file.Value;
						Output.Report($"Update file '{dstPath}'");
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
