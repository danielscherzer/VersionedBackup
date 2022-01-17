using VersionedCopy.Interfaces;

namespace VersionedCopyTests.Services
{
	internal class Dirs : IDirectories
	{
		public string DestinationDirectory { get; } = "dst";
		public string OldFilesFolder { get; } = "old";
		public string SourceDirectory { get; } = "src";
	}
}