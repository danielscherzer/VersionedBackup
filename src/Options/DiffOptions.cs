using CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace VersionedCopy.Options
{
	[Verb("diff", HelpText = "Create a difference of the directory store it into a zip.")]
	public class DiffOptions
	{
		public DiffOptions(string directory, string fileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool readOnly)
		{
			Directory = directory;
			FileName = fileName;
			IgnoreDirectories = ignoreDirectories;
			IgnoreFiles = ignoreFiles;
			ReadOnly = readOnly;
		}

		[Value(0, Required = true, HelpText = "The root directory of the directory tree to store.")]
		public string Directory { get; } = "";

		[Value(1, Required = true, HelpText = "The file to store the directory tree information.")]
		public string FileName { get; } = "";

		[Option(longName: "ignoreDirectories", Required = false, HelpText = "A list of ignored directories.")]
		public IEnumerable<string> IgnoreDirectories { get; } = Enumerable.Empty<string>();

		[Option(longName: "ignoreFiles", Required = false, HelpText = "A list of ignored files.")]
		public IEnumerable<string> IgnoreFiles { get; } = Enumerable.Empty<string>();

		[Option(longName: "readOnly", Default = false, Required = false, HelpText = "Only list operations. Do not change file system.")]
		public bool ReadOnly { get; }
	}
}
