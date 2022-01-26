using CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class ProgramTests
	{
		[DataTestMethod()]
		[DataRow("mirror", "c:\\src", "d:\\dst")]
		[DataRow("update", "c:\\src", "d:\\dst")]
		[DataRow("sync", "c:\\src", "d:\\dst")]
		[DataRow("sync", "d:\\daten", "e:\\daten", "--dryRun")]
		[DataRow("sync", "d:\\daten", "e:\\daten", "--dryRun", "--ignoreDirectories", ".vs", "bin", "obj", "TestResults", "--ignoreFiles", "desktop.ini", @"Visual Studio 2022\Visualizers\attribcache140.bin")]
		public void ParseArgumentsTest(params string[] args)
		{
			void AllTest(IOptions options)
			{
				Assert.AreEqual(args[1].IncludeTrailingPathDelimiter(), options.SourceDirectory);
				Assert.AreEqual(args[2].IncludeTrailingPathDelimiter(), options.DestinationDirectory);
				if (args.Length > 3) Assert.IsTrue(options.DryRun);
			}
			void SyncTest(SyncOptions options)
			{
				Assert.AreEqual(args[0], "sync");
				AllTest(options);
			}
			void UpdateTest(UpdateOptions options)
			{
				Assert.AreEqual(args[0], "update");
				AllTest(options);
			}

			void MirrorTest(MirrorOptions options)
			{
				Assert.AreEqual(args[0], "mirror");
				AllTest(options);
			}
			void Error(IEnumerable<Error> errors)
			{
				Assert.Fail(string.Join('\n', errors.Select(error => error.ToString())));
			}
			Parser.Default.ParseArguments<MirrorOptions, UpdateOptions, SyncOptions>(args)
				.WithParsed<MirrorOptions>(MirrorTest)
				.WithParsed<UpdateOptions>(UpdateTest)
				.WithParsed<SyncOptions>(SyncTest)
				.WithNotParsed(Error);
		}
	}
}