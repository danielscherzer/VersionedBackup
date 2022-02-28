using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
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

		internal void Copy(string src, string dst, List<string> srcNew)
		{
			foreach (var fileName in srcNew)
			{
				if (Token.IsCancellationRequested) return;
				if (Snapshot.IsFile(fileName))
				{
					if(FileSystem.Copy(src + fileName, dst + fileName)) //copy new
					{
						Output.Report($"New file '{fileName}'");
					}
				}
				else
				{
					if(FileSystem.CreateDirectory(dst + fileName))
					{
						Output.Report($"Create directory '{fileName}'");
					}
				}
			}
		}

		internal void MoveAway(string root, List<string> srcToDelete)
		{
			foreach (var fileName in srcToDelete)
			{
				if (Token.IsCancellationRequested) return;
				//move deleted to old
				if (Snapshot.IsFile(fileName))
				{
					if(FileSystem.MoveFile(root + fileName, Options.OldFilesFolder + fileName))
					{
						Output.Report($"Backup file '{fileName}'");
					}
				}
				else
				{
					if(FileSystem.MoveDirectory(root + fileName, Options.OldFilesFolder + fileName))
					{
						Output.Report($"Delete directory '{fileName}'");
					}
				}
			}
		}

		internal void UpdateFiles(string src, string dst, IEnumerable<string> updatedFiles)
		{
			foreach (var fileName in updatedFiles)
			{
				if (Token.IsCancellationRequested) return;
				if (FileSystem.MoveFile(dst + fileName, Options.OldFilesFolder + fileName)) //move away old
				{
					if (FileSystem.Copy(src + fileName, dst + fileName)) //copy new
					{
						Output.Report($"Replace file '{fileName}'");
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
