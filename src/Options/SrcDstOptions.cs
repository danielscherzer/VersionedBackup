using CommandLine;
using System.Collections.Generic;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Options
{
	public class SrcDstOptions : Options, ISrcDstOptions
	{
		public SrcDstOptions(string sourceDirectory, string destinationDirectory
			, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool readOnly)
			: base(ignoreDirectories, ignoreFiles, readOnly)
		{
			SourceDirectory = sourceDirectory.IncludeTrailingPathDelimiter();
			DestinationDirectory = destinationDirectory.IncludeTrailingPathDelimiter();
		}

		[Value(0, Required = true, HelpText = "The source directory of the to copy operation.")]
		public string SourceDirectory { get; } = "";

		[Value(1, Required = true, HelpText = "The destination directory of the copy operation.")]
		public string DestinationDirectory { get; } = "";
	}
}
