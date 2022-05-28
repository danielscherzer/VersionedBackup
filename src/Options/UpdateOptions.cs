using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Options
{
	[Verb("update", HelpText = "Update the destination directory form the soure directory.")]
	public class UpdateOptions : SrcDstOptions
	{
		public UpdateOptions(string sourceDirectory, string destinationDirectory
			, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool readOnly)
			: base(sourceDirectory, destinationDirectory, ignoreDirectories, ignoreFiles, readOnly)
		{
		}
	}
}
