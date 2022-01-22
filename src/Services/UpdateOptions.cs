using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Services
{
	[Verb("update", HelpText = "Update the destination directory form the soure directory.")]
	public class UpdateOptions : Options
	{
		public UpdateOptions(string sourceDirectory, string destinationDirectory
			, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool dryRun)
			: base(sourceDirectory, destinationDirectory, ignoreDirectories, ignoreFiles, dryRun)
		{
		}
	}
}
