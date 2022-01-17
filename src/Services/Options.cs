using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services
{
	internal class Options : IOptions
	{
		public Options(string sourceDirectory, string destinationDirectory
			, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool dryRun)
		{
			if (sourceDirectory is null) return;
			if (destinationDirectory is null) return;

			SourceDirectory = Path.GetFullPath(sourceDirectory).IncludeTrailingPathDelimiter();
			DestinationDirectory = Path.GetFullPath(destinationDirectory).IncludeTrailingPathDelimiter();
			OldFilesFolder = $"{DestinationDirectory[0..^1]}.old{Path.DirectorySeparatorChar}{DateTime.Now:yyyy-MM-dd_HHmmss}{Path.DirectorySeparatorChar}";
			IgnoreDirectories = (ignoreDirectories ?? throw new ArgumentNullException(nameof(ignoreDirectories))).Select(dir => dir.NormalizePathDelimiter().IncludeTrailingPathDelimiter());
			IgnoreFiles = (ignoreFiles ?? throw new ArgumentNullException(nameof(ignoreFiles))).Select(file => file.NormalizePathDelimiter());
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

		[Option(longName: "dryRun", Default = false, Required = false, HelpText = "Only list operations. Do not change file system.")]
		public bool DryRun { get; }

		[Option(longName: "mode", Default = false, Required = false, HelpText = "Choose copy mode from mirror source to destination, or sync.")]
		public AlgoMode Mode { get; } = AlgoMode.Mirror;

		public string OldFilesFolder { get; } = "";
	}
}
