using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using VersionedBackup.Interfaces;
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
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var srcFiles = new string[] { "d", "b", "c", "z" };
			foreach (var file in srcFiles) fileSystem.CreateFile(Path.Combine(dirs.SourceDirectory, file));
			Backup.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			foreach (var file in srcFiles) Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(dirs.DestinationDirectory, file)));
		}

		[TestMethod()]
		public void RunCopyTest()
		{
			var fileSystem = new VirtualFileSystem();
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			var srcDirs = new string[] { "a", dirs.SourceDirectory + "a", "b", "c", "d" };
			foreach (var subDir in srcDirs) fileSystem.CreateDirectory(Path.Combine(dirs.SourceDirectory, subDir));
			var srcFiles = new string[] { "x", "y", "z" };
			foreach (var file in srcFiles) fileSystem.CreateFile(Path.Combine(dirs.SourceDirectory, srcDirs[0], file));
			Backup.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			foreach (var file in srcFiles) Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(dirs.DestinationDirectory, srcDirs[0], file)));
			foreach (var subDir in srcDirs) Assert.IsTrue(fileSystem.ExistsDirectory(Path.Combine(dirs.DestinationDirectory, subDir)));
		}

		[TestMethod()]
		public void RunBigCopyTest()
		{
			var fileSystem = new VirtualFileSystem();
			List<string> srcFiles = new();
			List<string> srcSubDirs = new();
			Random rnd = new(21);
			int relativeStart = dirs.SourceDirectory.Length + 1;
			void FillFileSystem(string dir)
			{
				// subdirs
				for (int i = rnd.Next(0, 10); i > 0; --i)
				{
					char c = (char)((int)'A' + i);
					string path = Path.Combine(dir, c.ToString());
					fileSystem.CreateDirectory(path);
					srcSubDirs.Add(path[relativeStart..]);
					if (0 == rnd.Next(10)) FillFileSystem(path); // recursion
				}
				// files
				for (int i = rnd.Next(0, 30); i > 0; --i)
				{
					string path = Path.Combine(dir, i.ToString());
					fileSystem.CreateFile(path);
					srcFiles.Add(path[relativeStart..]);
				}
			}
			fileSystem.CreateDirectory(dirs.SourceDirectory);
			FillFileSystem(dirs.SourceDirectory);

			Backup.Run(new TestOptions(dirs), nullReport, fileSystem, token);
			foreach (var file in srcFiles) Assert.IsTrue(fileSystem.ExistsFile(Path.Combine(dirs.DestinationDirectory, file)));
			foreach (var subDir in srcSubDirs) Assert.IsTrue(fileSystem.ExistsDirectory(Path.Combine(dirs.DestinationDirectory, subDir)));
		}
	}
}