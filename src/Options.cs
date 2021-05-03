using CommandLine;
using System.IO;

namespace VersionedCopy
{
	internal class Options
	{
		private string _sourceDirectory = "";
		private string _destinationDirectory = "";

		[Value(0, Required = true, HelpText = "The source directory of the to copy operation.")]
		public string SourceDirectory
		{
			get => _sourceDirectory;
			set
			{
				_sourceDirectory = Path.GetFullPath(value).IncludeTrailingPathDelimiter();
				if (!Directory.Exists(_sourceDirectory))
				{
					Log.Print($"Source directory '{_sourceDirectory}' does not exist");
					return;
				}
			}
		}

		[Value(1, Required = true, HelpText = "The destination directory of the copy operation.")]
		public string DestinationDirectory
		{
			get => _destinationDirectory;
			set
			{
				_destinationDirectory = Path.GetFullPath(value).IncludeTrailingPathDelimiter();
				Directory.CreateDirectory(_destinationDirectory);
			}
		}
	}
}
