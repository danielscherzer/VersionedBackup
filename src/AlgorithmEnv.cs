using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
		/// <summary>
		/// optimize common update case: assume two directory structures are very similar
		/// 1=> read multi-threaded all directories and then all files
		/// 2=> optimize compare of almost all files for date time and size
		/// </summary>
		/// <param name="options"></param>
		/// <param name="token"><see cref="CancellationToken"/></param>
		public AlgorithmEnv(IOptions options, IOutput output, IFileSystem fileSystem, CancellationToken token)
		{
			Op = new Operations(output, options, fileSystem);
			Options = options;
			Output = output;
			FileSystem = fileSystem;
			Token = token;
		}

		public Task<string[]> EnumerateDirsAsync(string directory)
		{
			return Task.Run(FileSystem.EnumerateDirsRecursive(directory)
				.Ignore(Options.IgnoreDirectories).ToArray, Token);
		}

		public Task<HashSet<string>> EnumerateRelativeFilesAsync(string root, IEnumerable<string> directories)
		{
			root = root.IncludeTrailingPathDelimiter();
			return Task.Run(()
				=> FileSystem.EnumerateFiles(directories).Ignore(Options.IgnoreFiles).ToRelative(root)
				.ToHashSet(), Token);
		}

		internal void Copy(string src, string dst, IEnumerable<Entry> srcNew, Snapshot snapDst)
		{
			foreach (var file in srcNew)
			{
				if (Token.IsCancellationRequested) return;
				var fileName = file.Key;
				if (Snapshot.IsFile(fileName))
				{
					if (FileSystem.Copy(src + fileName, dst + fileName)) //copy new
					{
						snapDst.Entries[fileName] = file.Value;
						Output.Report($"New file '{dst + fileName}'");
					}
				}
				else
				{
					if(FileSystem.CreateDirectory(dst + fileName))
					{
						snapDst.Entries[fileName] = file.Value;
						Output.Report($"Create directory '{dst + fileName}'");
					}
				}
			}
		}

		internal void MoveAway(string root, IEnumerable<Entry> srcToDelete, Snapshot snapshot)
		{
			foreach (var file in srcToDelete)
			{
				if (Token.IsCancellationRequested) return;
				var fileName = file.Key;
				//move deleted to old
				if (Snapshot.IsFile(fileName))
				{
					if(FileSystem.MoveFile(root + fileName, Options.OldFilesFolder + fileName))
					{
						snapshot.Entries.Remove(fileName);
						Output.Report($"Backup deleted file '{root + fileName}'");
					}
				}
				else
				{
					if(FileSystem.MoveDirectory(root + fileName, Options.OldFilesFolder + fileName))
					{
						snapshot.Entries.Remove(fileName);
						Output.Report($"Backup deleted directory '{root + fileName}'");
					}
				}
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
		public Operations Op { get; }
	}
}
