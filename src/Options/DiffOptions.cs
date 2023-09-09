using CommandLine;
using System.Collections.Generic;

namespace VersionedCopy.Options;

[Verb("diff", HelpText = "Create a difference of the directory store it into a zip.")]
public class DiffOptions : Options
{
	public DiffOptions(string directory, string fileName, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, bool readOnly)
		: base(ignoreDirectories, ignoreFiles, readOnly)
	{
		Directory = directory;
		FileName = fileName;
	}

	[Value(0, Required = true, HelpText = "The root directory of the directory tree to store.")]
	public string Directory { get; } = "";

	[Value(1, Required = true, HelpText = "The file to store the directory tree information.")]
	public string FileName { get; } = "";
}
