using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VersionedCopy
{
	internal class Options
	{
		public Options(string sourceDirectory, string destinationDirectory
			, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		{
			if (sourceDirectory is null) return;
			if (destinationDirectory is null) return;
			SourceDirectory = Path.GetFullPath(sourceDirectory).IncludeTrailingPathDelimiter();
			if (!Directory.Exists(SourceDirectory))
			{
				Log.Print($"Source directory '{SourceDirectory}' does not exist");
				return;
			}
			DestinationDirectory = Path.GetFullPath(destinationDirectory).IncludeTrailingPathDelimiter();
			FileSystem.CreateDirectory(DestinationDirectory);

			OldFilesFolder = $"{DestinationDirectory[0..^1]}-{DateTime.Now:yyyy-MM-dd_HHmmss}{Path.DirectorySeparatorChar}";

			IgnoreDirectories = ignoreDirectories ?? throw new ArgumentNullException(nameof(ignoreDirectories));
			IgnoreFiles = ignoreFiles ?? throw new ArgumentNullException(nameof(ignoreFiles));
		}

		[Value(0, Required = true, HelpText = "The source directory of the to copy operation.")]
		public string SourceDirectory { get; } = "";

		[Value(1, Required = true, HelpText = "The destination directory of the copy operation.")]
		public string DestinationDirectory { get; } = "";

		/// <summary>
		/// List of directories including trailing path delimiter
		/// </summary>
		[Option(longName: "ignoreDirectories", Required = false, HelpText = "A list of ignored files.")]
		public IEnumerable<string> IgnoreDirectories { get; } = Enumerable.Empty<string>();

		[Option(longName: "ignoreFiles", Required = false, HelpText = "A list of ignored directories.")]
		public IEnumerable<string> IgnoreFiles { get; } = Enumerable.Empty<string>();

		internal string OldFilesFolder { get; } = "";
	}
}
