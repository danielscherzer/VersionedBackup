using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class DiffTests
	{
		[TestMethod(), TestCategory("Hack")]
		public void RunTest()
		{
			//Program.Main(new string[] { "diff", @"d:\daten", @"d:\diff.zip", "--ignoreDirectories", ".vs", "bin", "obj", "TestResults", });
		}

		[TestMethod(), TestCategory("Hack")]
		public void LoadTest()
		{
			//Program.Main(new string[] { "diffmerge", @"d:\daten", @"d:\diff.zip", "--ignoreDirectories", ".vs", "bin", "obj", "TestResults", });
		}
	}
}