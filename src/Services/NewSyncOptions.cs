using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Services
{
	[Verb("newsync", HelpText = "Sync the soure directory and the destination directory bidirectionally.")]
	public class NewSyncOptions : Options
	{
		public NewSyncOptions(string sourceDirectory, string destinationDirectory, string oldFilesFolder, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool dryRun) : base(sourceDirectory, destinationDirectory, oldFilesFolder, ignoreDirectories, ignoreFiles, dryRun)
		{
		}
	}
}
