using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Options
{
	[Verb("sync", HelpText = "Sync the soure directory and the destination directory bidirectionally.")]
	public class SyncOptions : Options
	{
		public SyncOptions(string sourceDirectory, string destinationDirectory, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool readOnly) 
			: base(sourceDirectory, destinationDirectory, ignoreDirectories, ignoreFiles, readOnly)
		{
		}
	}
}
