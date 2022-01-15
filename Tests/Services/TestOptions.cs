using System.Collections.Generic;
using System.Linq;
using VersionedBackup.Interfaces;

namespace VersionedBackupTests.Services
{
	internal class TestOptions : IOptions
	{
		private readonly IDirectories dirs;

		public TestOptions(IDirectories dirs)
		{
			this.dirs = dirs;
		}

		public IEnumerable<string> IgnoreDirectories => Enumerable.Empty<string>();
		public IEnumerable<string> IgnoreFiles => Enumerable.Empty<string>();
		public bool DryRun => false;
		public string DestinationDirectory => dirs.DestinationDirectory;
		public string OldFilesFolder => dirs.OldFilesFolder;
		public string SourceDirectory => dirs.SourceDirectory;
	}
}