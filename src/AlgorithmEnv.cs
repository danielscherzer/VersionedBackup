using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;

namespace VersionedCopy
{
	using Entry = KeyValuePair<string, DateTime>;

	public class AlgorithmEnv
	{
		/// <summary>
		/// optimize common update case: assume two directory structures are very similar
		/// 1=> read multi-threaded all directories and then all files
		/// 2=> optimize compare of almost all files for date time and size
		/// </summary>
		/// <param name="options"></param>
		/// <param name="token"><see cref="CancellationToken"/></param>
		public AlgorithmEnv(IOptions options, IOutput output, IFileSystem fileSystem, CancellationToken token)
		{
			Options = options;
			Output = output;
			FileSystem = fileSystem;
			Token = token;
			if (!Directory.Exists(options.DestinationDirectory))
			{
				if (fileSystem.CreateDirectory(options.DestinationDirectory))
				{
					output.Report($"Create directory '{options.DestinationDirectory}'");
				}
			}
		}

		internal void Copy(string src, string dst, IEnumerable<Entry> srcNew)
		{
			try
			{
				// create directories
				Parallel.ForEach(srcNew.Where(file => !Snapshot.IsFile(file.Key)), new ParallelOptions { CancellationToken = Token }, file =>
				{
					var fileName = file.Key;
					if (FileSystem.CreateDirectory(dst + fileName))
					{
						Output.Report($"Create directory '{dst + fileName}'");
					}
				});
				// files
				Parallel.ForEach(srcNew.Where(file => Snapshot.IsFile(file.Key)), new ParallelOptions { CancellationToken = Token }, file =>
				{
					var fileName = file.Key;
					if (FileSystem.Copy(src + fileName, dst + fileName))
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
				if (FileSystem.CreateDirectory(dst + fileName))
				{
					snapDst.Entries[fileName] = file.Value;
					Output.Report($"Create directory '{dst + fileName}'");
				}
			}
			foreach (var file in srcNew.Where(file => Snapshot.IsFile(file.Key)))
			{
				var fileName = file.Key;
				if (Token.IsCancellationRequested) return;
				if (FileSystem.Copy(src + fileName, dst + fileName))
				{
					snapDst.Entries[fileName] = file.Value;
					Output.Report($"New file '{dst + fileName}'");
				}
			}
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
					if (FileSystem.MoveFile(root + fileName, Options.OldFilesFolder + fileName))
					{
						snapshot.Entries.Remove(fileName);
						Output.Report($"Backup deleted file '{root + fileName}'");
					}
				}
				else
				{
					if (FileSystem.MoveDirectory(root + fileName, Options.OldFilesFolder + fileName))
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
					if (FileSystem.MoveFile(dst + fileName, Options.OldFilesFolder + fileName)) //move away old
					{
						if (FileSystem.Copy(src + fileName, dst + fileName)) //copy new
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
				if (FileSystem.MoveFile(dst + fileName, Options.OldFilesFolder + fileName)) //move away old
				{
					if (FileSystem.Copy(src + fileName, dst + fileName)) //copy new
					{
						snapDst.Entries[fileName] = file.Value;
						Output.Report($"Update file '{dst + fileName}'");
					}
				}
			}
		}

		public IOptions Options { get; }
		public IOutput Output { get; }
		public IFileSystem FileSystem { get; }
		public CancellationToken Token { get; }
	}
}
