using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
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

		public const string FileNameSnapShot = ".versioned.copy.snapshot.json";

		internal void Copy(string src, string dst, IEnumerable<Entry> srcNew)
		{
			try
			{
				// create directories
				Parallel.ForEach(srcNew.Where(file => !Snapshot.IsFile(file.Key)), new ParallelOptions { CancellationToken = Token }, file =>
				{
					var fileName = file.Key;
					if (FileOperations.CreateDirectory(dst + fileName))
					{
						Output.Report($"Create directory '{dst + fileName}'");
					}
				});
				// files
				Parallel.ForEach(srcNew.Where(file => Snapshot.IsFile(file.Key)), new ParallelOptions { CancellationToken = Token }, file =>
				{
					var fileName = file.Key;
					if (FileOperations.Copy(src + fileName, dst + fileName))
					{
						Output.Report($"New file '{dst + fileName}'");
					}
				});
			}
			catch (OperationCanceledException)
			{
			}
		}

		internal void Copy(string src, string dst, IEnumerable<Entry> srcNew, Snapshot snapDst)
		{
			foreach (var file in srcNew.Where(file => !Snapshot.IsFile(file.Key)))
			{
				var fileName = file.Key;
				if (Token.IsCancellationRequested) return;
				if (FileOperations.CreateDirectory(dst + fileName))
				{
					snapDst.Entries[fileName] = file.Value;
					Output.Report($"Create directory '{dst + fileName}'");
				}
			}
			foreach (var file in srcNew.Where(file => Snapshot.IsFile(file.Key)))
			{
				var fileName = file.Key;
				if (Token.IsCancellationRequested) return;
				if (FileOperations.Copy(src + fileName, dst + fileName))
				{
					snapDst.Entries[fileName] = file.Value;
					Output.Report($"New file '{dst + fileName}'");
				}
			}
		}

		internal static Snapshot? LoadSnapshot(string root)
		{
			return Persist.Load<Snapshot>(Path.Combine(root, FileNameSnapShot));
		}

		internal static void SaveSnapshot(Snapshot snap, string root) => Persist.Save(snap, Path.Combine(root, FileNameSnapShot));

		internal Snapshot CreateSnapshot(string root)
		{
			return Snapshot.Create(root, Options.IgnoreDirectories, Options.IgnoreFiles, Token);
		}

		internal void MoveAway(string root, IEnumerable<Entry> toDelete, Snapshot snapshot)
		{
			foreach (var file in toDelete)
			{
				if (Token.IsCancellationRequested) return;
				var fileName = file.Key;
				//move deleted to old
				if (Snapshot.IsFile(fileName))
				{
					if (FileOperations.MoveFile(root + fileName, Options.OldFilesFolder + fileName)) //TODO: if different drives, move will not work
					{
						snapshot.Entries.Remove(fileName);
						Output.Report($"Backup deleted file '{root + fileName}'");
					}
				}
				else
				{
					if (FileOperations.MoveDirectory(root + fileName, Options.OldFilesFolder + fileName))
					{
						snapshot.Entries.Remove(fileName);
						Output.Report($"Backup deleted directory '{root + fileName}'");
					}
				}
			}
		}

		internal void SetTimeStamp(string fileName, DateTime newTime)
		{
			Output.Report($"New time stamp '{fileName}'");
			if (Options.ReadOnly) return;
			if (Snapshot.IsFile(fileName))
			{
				File.SetLastWriteTimeUtc(fileName, newTime);
			}
			else
			{
				Directory.SetCreationTimeUtc(fileName, newTime);
			}
		}

		internal void UpdateFiles(string src, string dst, IEnumerable<Entry> updatedFiles)
		{
			try
			{
				Parallel.ForEach(updatedFiles, new ParallelOptions { CancellationToken = Token }, file =>
				{
					var fileName = file.Key;
					if (FileOperations.MoveFile(dst + fileName, Options.OldFilesFolder + fileName)) //move away old
					{
						if (FileOperations.Copy(src + fileName, dst + fileName)) //copy new
						{
							Output.Report($"Update file '{dst + fileName}'");
						}
					}
				});
			}
			catch (OperationCanceledException)
			{
			}
		}

		internal void UpdateFiles(string src, string dst, IEnumerable<Entry> updatedFiles, Snapshot snapDst)
		{
			foreach (var file in updatedFiles)
			{
				if (Token.IsCancellationRequested) return;
				var fileName = file.Key;
				if (FileOperations.MoveFile(dst + fileName, Options.OldFilesFolder + fileName)) //move away old
				{
					if (FileOperations.Copy(src + fileName, dst + fileName)) //copy new
					{
						snapDst.Entries[fileName] = file.Value;
						Output.Report($"Update file '{dst + fileName}'");
					}
				}
			}
		}

		public IOptions Options { get; }
		public CancellationToken Token { get; }
		
		private FileOperations FileOperations { get; }
		private IOutput Output { get; }
	}
}
