using CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;
using VersionedCopyTests.Services;

namespace VersionedCopy.Program.Tests
{
	[TestClass()]
	public class ProgramTests
	{
		[DataTestMethod()]
		[DataRow("c:\\src", "d:\\dst")]
		[DataRow("c:\\src", "d:\\dst", "--mode=Sync")]
		public void ParseArgumentsTest(params string[] args)
		{
			void Run(IOptions options)
			{
				Assert.AreEqual(args[0].IncludeTrailingPathDelimiter(), options.SourceDirectory);
				Assert.AreEqual(args[1].IncludeTrailingPathDelimiter(), options.DestinationDirectory);
				if (args.Length > 2) Assert.AreEqual(AlgoMode.Sync, options.Mode);
			}
			void Error(IEnumerable<Error> errors)
			{
				Assert.Fail(string.Join('\n', errors.Select(error => error.ToString())));
			}
			try
			{
				ParserResult<Options> parserResult = Parser.Default.ParseArguments<Options>(args);
				parserResult.WithParsed(Run);
				parserResult.WithNotParsed(Error);
			}
			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
		}

		[TestMethod()]
		public void RunEmptyTest()
		{
			var fileSystem = new VirtualFileSystem();
			var report = new NullReport();
			Assert.ThrowsException<Exception>(() =>
			{
				if (!fileSystem.ExistsDirectory(""))
				{
					report.Error("");
				}
			});
		}
	}
}