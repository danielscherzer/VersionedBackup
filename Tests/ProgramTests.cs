using CommandLine;
using CommandLine.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy.Program.Tests
{
	[TestClass()]
	public class ProgramTests
	{
		[DataTestMethod()]
		[DataRow(new string[] { "c:\\src", "d:\\dst" })]
		[DataRow(new string[] { "c:\\src", "d:\\dst", "--mode 0" })]
		public void ParseArgumentsTest(string[] args)
		{
			void Run(IOptions options)
			{
				Assert.AreEqual(args[0].IncludeTrailingPathDelimiter(), options.SourceDirectory);
				Assert.AreEqual(args[1].IncludeTrailingPathDelimiter(), options.DestinationDirectory);
				Assert.AreEqual(args[2], options.Mode);
			}
			void Error(IEnumerable<Error> errors)
			{
				//Assert.Fail(string.Join('\n', errors.Select(error => error.ToString())));
			}
			try
			{
				ParserResult<Options> parserResult = Parser.Default.ParseArguments<Options>(args);
				parserResult.WithParsed(Run);
				parserResult.WithNotParsed(Error);
			}
			catch(Exception e)
			{
				Assert.Fail(e.Message);
			}
		}
	}
}