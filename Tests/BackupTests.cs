using Microsoft.VisualStudio.TestTools.UnitTesting;
using VersionedBackup.Interfaces;
using VersionedBackup.Services;
using VersionedBackupTests.Services;

namespace VersionedBackup.Tests
{
	[TestClass()]
	public class BackupTests
	{
		private readonly IDirectories dirs = new Dirs();
		private readonly IReport nullReport = new NullReport();

		[TestMethod()]
		public void RunTest()
		{
			var fileSystem = new ListFileSystem();
			var op = new FileSystemOperations(nullReport, dirs, fileSystem);
			Backup.Run(new TestOptions(dirs), op, fileSystem, new System.Threading.CancellationToken());
		}
	}
}