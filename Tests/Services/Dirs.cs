using VersionedBackup.Interfaces;
using VersionedBackup.PathHelper;

namespace VersionedBackupTests.Services
{
	internal class Dirs : IDirectories
	{
		public string DestinationDirectory { get; } = "dst".IncludeTrailingPathDelimiter();
		public string OldFilesFolder { get; } = "old".IncludeTrailingPathDelimiter();
		public string SourceDirectory { get; } = "src".IncludeTrailingPathDelimiter();
	}
}