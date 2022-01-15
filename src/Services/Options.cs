using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VersionedBackup.Interfaces;
using VersionedBackup.PathHelper;

namespace VersionedBackup.Services
{
	internal class Options : IOptions
	{
		public Options(string sourceDirectory, string destinationDirectory
			, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool logIgnoreOperations, bool logIgnoreErrors, bool dryRun)
		{
			if (sourceDirectory is null) return;
			if (destinationDirectory is null) return;

			SourceDirectory = Path.GetFullPath(sourceDirectory).IncludeTrailingPathDelimiter();
			DestinationDirectory = Path.GetFullPath(destinationDirectory).IncludeTrailingPathDelimiter();
			OldFilesFolder = $"{DestinationDirectory[0..^1]}-old{Path.DirectorySeparatorChar}{DateTime.Now:yyyy-MM-dd_HHmmss}{Path.DirectorySeparatorChar}";
			IgnoreDirectories = (ignoreDirectories ?? throw new ArgumentNullException(nameof(ignoreDirectories))).Select(dir => dir.NormalizePathDelimiter().IncludeTrailingPathDelimiter());
			IgnoreFiles = (ignoreFiles ?? throw new ArgumentNullException(nameof(ignoreFiles))).Select(file => file.NormalizePathDelimiter());
			LogIgnoreOperations = logIgnoreOperations;
			LogIgnoreErrors = logIgnoreErrors;
			DryRun = dryRun;
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

		[Option(longName: "dryRun", Default = false, Required = false, HelpText = "Only list operations. Do not change file system.")]
		public bool DryRun { get; }

		public string OldFilesFolder { get; } = "";

		public bool LogErrors => !LogIgnoreErrors;

		public bool LogOperations => !LogIgnoreOperations;
	}
}
