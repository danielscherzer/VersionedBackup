using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Services
{
	[Verb("mirror", HelpText = "Mirror the soure directory to the destination directory.")]
	public class MirrorOptions : Options
	{
		public MirrorOptions(string sourceDirectory, string destinationDirectory, string oldFilesFolder, IEnumerable<string> ignoreDirectories
			, IEnumerable<string> ignoreFiles, bool readOnly)
			: base(sourceDirectory, destinationDirectory, oldFilesFolder, ignoreDirectories, ignoreFiles, readOnly)
		{
		}
	}
}
