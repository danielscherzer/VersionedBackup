using CommandLine;
using System.Collections.Generic;
using System.Linq;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Options
{
	public class Options : IOptions
	{
		public Options(IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool readOnly)
		{
			IgnoreDirectories = ignoreDirectories.Select(dir => dir.NormalizePathDelimiter().IncludeTrailingPathDelimiter());
			IgnoreFiles = ignoreFiles.Select(file => file.NormalizePathDelimiter());
			ReadOnly = readOnly;
		}

		[Option(longName: "ignoreDirectories", Required = false, HelpText = "A list of ignored directories.")]
		public IEnumerable<string> IgnoreDirectories { get; } = Enumerable.Empty<string>();

		[Option(longName: "ignoreFiles", Required = false, HelpText = "A list of ignored files.")]
		public IEnumerable<string> IgnoreFiles { get; } = Enumerable.Empty<string>();

		[Option(longName: "readOnly", Default = false, Required = false, HelpText = "Only list operations. Do not change file system.")]
		public bool ReadOnly { get; }
	}
}
