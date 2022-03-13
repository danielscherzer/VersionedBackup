using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Options
{
	[Verb("mirror", HelpText = "Mirror the soure directory to the destination directory.")]
	public class MirrorOptions : Options
	{
		public MirrorOptions(string sourceDirectory, string destinationDirectory, IEnumerable<string> ignoreDirectories
			, IEnumerable<string> ignoreFiles, bool readOnly)
			: base(sourceDirectory, destinationDirectory, ignoreDirectories, ignoreFiles, readOnly)
		{
		}
	}
}
