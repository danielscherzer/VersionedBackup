using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Services
{
	[Verb("sync", HelpText = "Sync the soure directory and the destination directory bidirectionally.")]
	public class SyncOptions : Options
	{
		public SyncOptions(string sourceDirectory, string destinationDirectory, string oldFilesFolder, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool dryRun) : base(sourceDirectory, destinationDirectory, oldFilesFolder, ignoreDirectories, ignoreFiles, dryRun)
		{
		}
	}
}
