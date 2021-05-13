
using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VersionedCopy.Interfaces;

namespace VersionedCopy.Services
{
	internal class Options : IOptions
	{
		public Options(string sourceDirectory, string destinationDirectory
			, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool logIgnoreOperations, bool logIgnoreErrors)
		{
			if (sourceDirectory is null) return;
			if (destinationDirectory is null) return;

			SourceDirectory = Path.GetFullPath(sourceDirectory).IncludeTrailingPathDelimiter();
			DestinationDirectory = Path.GetFullPath(destinationDirectory).IncludeTrailingPathDelimiter();
			OldFilesFolder = $"{DestinationDirectory[0..^1]}-{DateTime.Now:yyyy-MM-dd_HHmmss}{Path.DirectorySeparatorChar}";
			IgnoreDirectories = ignoreDirectories ?? throw new ArgumentNullException(nameof(ignoreDirectories));
			IgnoreFiles = ignoreFiles ?? throw new ArgumentNullException(nameof(ignoreFiles));
			LogIgnoreOperations = logIgnoreOperations;
			LogIgnoreErrors = logIgnoreErrors;
		}

		[Value(0, Required = true, HelpText = "The source directory of the to copy operation.")]
		public string SourceDirectory { get; } = "";

		[Value(1, Required = true, HelpText = "The destination directory of the copy operation.")]
		public string DestinationDirectory { get; } = "";

		[Option(longName: "ignoreDirectories", Required = false, HelpText = "A list of ignored files.")]
		public IEnumerable<string> IgnoreDirectories { get; } = Enumerable.Empty<string>();

		[Option(longName: "ignoreFiles", Required = false, HelpText = "A list of ignored directories.")]
		public IEnumerable<string> IgnoreFiles { get; } = Enumerable.Empty<string>();

		[Option(longName: "logIgnoreOperations", Default = false, Required = false, HelpText = "Log no file operations.")]
		public bool LogIgnoreOperations { get; }

		[Option(longName: "logIgnoreErrors", Default = false, Required = false, HelpText = "Log no errors.")]
		public bool LogIgnoreErrors { get; }

		public string OldFilesFolder { get; } = "";

		public bool LogErrors => !LogIgnoreErrors;

		public bool LogOperations => !LogIgnoreOperations;
	}
}
