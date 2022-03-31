using CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using VersionedCopy.Interfaces;
using VersionedCopy.Options;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class ParseTests
	{
		static bool IsReadOnly(string arg)
		{
			arg = arg.ToLowerInvariant();
			return arg.StartsWith("--read") || arg.StartsWith("-r");
		}

		static string Dir(string dir) => dir.IncludeTrailingPathDelimiter();

		static IEnumerable<string> TakeOption(IEnumerable<string> seq, string start)
		{
			return seq.SkipWhile(arg => !arg.ToLowerInvariant().StartsWith(start.ToLowerInvariant()))
				.Skip(1).TakeWhile(arg => !arg.StartsWith("-"));
		}

		[DataTestMethod()]
		[DataRow("mirror", "c:\\src", "d:\\dst")]
		[DataRow("update", "c:\\src", "d:\\dst")]
		[DataRow("sync", "c:\\src", "d:\\dst")]
		[DataRow("sync", "d:\\daten", "e:\\daten", "--Readonly")]
		[DataRow("sync", "d:\\daten", "e:\\daten", "--ignoreDirectories", "TestResults")]
		[DataRow("synC", "d:\\daten", "e:\\daten", "--ignoreDirectories", ".vs", "bin", "obj", "TestResults")]
		[DataRow("sync", "d:\\daten", "e:\\daten", "--ignoreFiles", "desktop.ini")]
		[DataRow("sync", "d:\\daten", "e:\\daten", "--ignoreFiles", "desktop.ini", @"Visual Studio 2022\Visualizers\attribcache140.bin")]
		[DataRow("sync", "d:\\daten", "e:\\daten", "--readOnly", "--ignoreDirectories", ".vs", "bin", "obj", "TestResults", "--ignoreFiles", "desktop.ini", @"Visual Studio 2022\Visualizers\attribcache140.bin")]
		[DataRow("Sync", "d:\\daten", "e:\\daten", "--ignoreFiles", "desktop.ini", "--ignoreDirectories", ".vs")]
		public void ParseArgumentsTest(params string[] args)
		{
			void AllTest(ISrcDstOptions options)
			{
				Assert.AreEqual(Dir(args[1]), options.SourceDirectory);
				Assert.AreEqual(Dir(args[2]), options.DestinationDirectory);
				Assert.AreEqual(args.Any(arg => IsReadOnly(arg)), options.ReadOnly);

				var ignoreDirs = TakeOption(args, "--ignoreD").Select(dir => Dir(dir)).ToArray();
				if (ignoreDirs.Any())
				{
					CollectionAssert.AreEquivalent(ignoreDirs, options.IgnoreDirectories.ToArray());
				}
				var ignoreFiles = TakeOption(args, "--ignoreF").ToArray();
				if (ignoreFiles.Any())
				{
					CollectionAssert.AreEquivalent(ignoreFiles, options.IgnoreFiles.ToArray());
				}
			}
			void Verb(string verb, ISrcDstOptions options)
			{
				Assert.AreEqual(args[0].ToLowerInvariant(), verb);
				AllTest(options);
			}
			void Error(IEnumerable<Error> errors)
			{
				Assert.Fail(string.Join('\n', errors.Select(error => error.ToString())));
			}
			Program.Parse(args)
				.WithParsed<MirrorOptions>(opt => Verb("mirror", opt))
				.WithParsed<UpdateOptions>(opt => Verb("update", opt))
				.WithParsed<SyncOptions>(opt => Verb("sync", opt))
				.WithNotParsed(Error);
		}
	}
}