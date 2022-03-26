using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Options
{
	public class Options : IOptions
	{
		public Options(string sourceDirectory, string destinationDirectory
			, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool readOnly)
		{
			SourceDirectory = sourceDirectory.IncludeTrailingPathDelimiter();
			DestinationDirectory = destinationDirectory.IncludeTrailingPathDelimiter();
			IgnoreDirectories = (ignoreDirectories ?? throw new ArgumentNullException(nameof(ignoreDirectories))).Select(dir => dir.NormalizePathDelimiter().IncludeTrailingPathDelimiter());
			IgnoreFiles = (ignoreFiles ?? throw new ArgumentNullException(nameof(ignoreFiles))).Select(file => file.NormalizePathDelimiter());
			ReadOnly = readOnly;
		}

		[Value(0, Required = true, HelpText = "The source directory of the to copy operation.")]
		public string SourceDirectory { get; } = "";

		[Value(1, Required = true, HelpText = "The destination directory of the copy operation.")]
		public string DestinationDirectory { get; } = "";

		[Option(longName: "ignoreDirectories", Required = false, HelpText = "A list of ignored directories.")]
		public IEnumerable<string> IgnoreDirectories { get; } = Enumerable.Empty<string>();

		[Option(longName: "ignoreFiles", Required = false, HelpText = "A list of ignored files.")]
		public IEnumerable<string> IgnoreFiles { get; } = Enumerable.Empty<string>();

		[Option(longName: "readOnly", Default = false, Required = false, HelpText = "Only list operations. Do not change file system.")]
		public bool ReadOnly { get; }
	}
}
