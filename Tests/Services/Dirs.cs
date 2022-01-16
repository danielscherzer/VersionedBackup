using VersionedBackup.Interfaces;

namespace VersionedBackupTests.Services
{
	internal class Dirs : IDirectories
	{
		public string DestinationDirectory { get; } = "dst";
		public string OldFilesFolder { get; } = "old";
		public string SourceDirectory { get; } = "src";
	}
}