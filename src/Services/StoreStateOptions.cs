using CommandLine;
using System.Collections.Generic;
using System.Linq;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services
{
	public class StoreStateOptions
	{
		public StoreStateOptions(string directory, string databaseFileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles)
		{
			Directory = directory.IncludeTrailingPathDelimiter();
			DatabaseFileName = databaseFileName;
			IgnoreDirectories = ignoreDirectories;
			IgnoreFiles = ignoreFiles;
		}

		[Value(0, Required = true, HelpText = "The root directory of the directory tree to store.")]
		public string Directory { get; } = "";

		[Value(1, Required = true, HelpText = "The file to store the directory tree information.")]
		public string DatabaseFileName { get; } = "";

		[Option(longName: "ignoreDirectories", Required = false, HelpText = "A list of ignored directories.")]
		public IEnumerable<string> IgnoreDirectories { get; } = Enumerable.Empty<string>();

		[Option(longName: "ignoreFiles", Required = false, HelpText = "A list of ignored files.")]
		public IEnumerable<string> IgnoreFiles { get; } = Enumerable.Empty<string>();
	}
}
