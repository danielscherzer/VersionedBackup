using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Services
{
	[Verb("mirror", HelpText = "Mirror the soure directory to the destination directory.")]
	public class MirrorOptions : Options
	{
		public MirrorOptions(string sourceDirectory, string destinationDirectory
			, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool dryRun) 
			: base(sourceDirectory, destinationDirectory, ignoreDirectories, ignoreFiles, dryRun)
		{
		}
	}
}
