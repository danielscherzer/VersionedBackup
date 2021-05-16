using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;

namespace VersionedCopy.PathHelper.Tests
{
	[TestClass()]
	public class PathHelperTests
	{
		[DataTestMethod()]
		// directories
		[DataRow(@".vs\", @"D:\Daten\.vs\_cmdTools\VersionedBackup\")]
		[DataRow(@".vs\", @"D:\Daten\.vscode\_cmdTools\VersionedBackup\", false)]
		[DataRow(@".vs\", @"D:\Daten\C#\_cmdTools\VersionedBackup\.vs\")]
		[DataRow(@".vs\", @"D:\Daten\C#\_cmdTools\VersionedBackup\test.vs\", false)]
		[DataRow(@"*.log\", @"\.log\sadf\autoexec\")]
		[DataRow(@"*.log\", @"\qwert.log\sadf\autoexec\")]
		[DataRow(@"*.log\", @"\qwert.logy\sadf\autoexec\", false)]
		// files
		[DataRow(@"log", @"\log")]
		[DataRow(@"log", @"\lag", false)]
		[DataRow(@"log", @"\loga", false)]
		[DataRow(@"?log", @"\alog")]
		[DataRow(@"??log", @"\alog", false)]
		[DataRow(@"log", @"\alog", false)]
		[DataRow(@"log", @"\loglog", false)]
		[DataRow(@"*.log", @"\autoexec.log")]
		[DataRow(@"a*.log", @"\butoexec.log", false)]
		[DataRow(@"b*.log", @"\butoexec.log")]
		[DataRow(@"*.lo", @"\autoexec.log", false)]
		[DataRow(@"\*.log", @"\autoexec.log")]
		[DataRow(@"\b*\*.log", @"\b\autoexec.log")]
		[DataRow(@"\how\*\*.log", @"\how\sadf\autoexec.log")]
		[DataRow(@"\*.log", @"\afsdfsf\sadf\autoexec.log", false)]
		public void WildcardToRegexTest(string pattern, string match, bool isMatch = true)
		{
			var regex = new Regex(pattern.WildcardToRegex(), RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
			if(isMatch) Assert.IsTrue(regex.IsMatch(match));
			else Assert.IsFalse(regex.IsMatch(match));
		}
	}
}