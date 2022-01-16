using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;
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
		private readonly CancellationToken token = new();

		[TestMethod()]
		public void RunEmptyTest()
		{
			var fileSystem = new VirtualFileSystem();
			Assert.ThrowsException<Exception>(() =>
			Backup.Run(new TestOptions(dirs), nullReport, fileSystem, token));
		}

		[TestMethod()]
		public void RunSrcExistsTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			Backup.Run(new TestOptions(dirs), nullReport, fileSystem, token);
		}

		[TestMethod()]
		public void RunCopyDirsTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var srcDirs = new string[] {"a", dirs.SourceDirectory + "a", "b", "_" };
			foreach (var subDir in srcDirs) fileSystem.CreateDirectory(Path.Combine(dirs.SourceDirectory, subDir));
			fileSystem.CreateDirectory(Path.Combine(dirs.DestinationDirectory, srcDirs[0]));
			Backup.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			foreach (var subDir in srcDirs) Assert.IsTrue(fileSystem.ExistsDirectory(Path.Combine(dirs.DestinationDirectory, subDir)));
		}


		[TestMethod()]
		public void RunCopyFilesTest()
		{
			//for(int i = 0; i < 10000; ++i)
			{
				var fileSystem = new VirtualFileSystem();
				fileSystem.CreateDirectory(dirs.SourceDirectory);
				var srcFiles = new string[] { "a", "b", "c" };
				foreach (var file in srcFiles) fileSystem.CreateFile(Path.Combine(dirs.SourceDirectory, file));
				Backup.Run(new TestOptions(dirs), nullReport, fileSystem, token);
				foreach (var file in srcFiles) Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(dirs.DestinationDirectory, file)));
			}
		}

		[TestMethod()]
		public void RunCopyTest()
		{
			//for (int i = 0; i < 100; ++i)
			{
				var fileSystem = new VirtualFileSystem();
				fileSystem.CreateDirectory(dirs.SourceDirectory);
				var srcDirs = new string[] { "a", dirs.SourceDirectory + "a", "b", "_" };
				foreach (var subDir in srcDirs) fileSystem.CreateDirectory(Path.Combine(dirs.SourceDirectory, subDir));
				var srcFiles = new string[] { "x", "y", "z" };
				foreach (var file in srcFiles) fileSystem.CreateFile(Path.Combine(dirs.SourceDirectory, srcDirs[0], file));
				Backup.Run(new TestOptions(dirs), nullReport, fileSystem, token);
				foreach (var file in srcFiles) Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(dirs.DestinationDirectory, srcDirs[0], file)));
				foreach (var subDir in srcDirs) Assert.IsTrue(fileSystem.ExistsDirectory(Path.Combine(dirs.DestinationDirectory, subDir)));
			}
		}
	}
}