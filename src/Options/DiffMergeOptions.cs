using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Options
{
	[Verb("diffMerge", HelpText = "Merge changes stored in the file into the directory.")]
	public class DiffMergeOptions : DiffOptions
	{
		public DiffMergeOptions(string directory, string fileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool readOnly)
			: base(directory, fileName, ignoreDirectories, ignoreFiles, readOnly)
		{
		}
	}
}
