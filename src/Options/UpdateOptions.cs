using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Options
{
	[Verb("update", HelpText = "Update the destination directory form the soure directory.")]
	public class UpdateOptions : Options
	{
		public UpdateOptions(string sourceDirectory, string destinationDirectory, string oldFilesFolder, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool readOnly) : base(sourceDirectory, destinationDirectory, oldFilesFolder, ignoreDirectories, ignoreFiles, readOnly)
		{
		}
	}
}
