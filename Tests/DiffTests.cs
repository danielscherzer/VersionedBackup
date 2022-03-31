using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class DiffTests
	{
		[TestMethod()]
		public void RunTest()
		{
			Program.Main(new string[] { "diff", @"d:\daten", @"d:\diff.zip", "--ignoreDirectories", ".vs", "bin", "obj", "TestResults", });
		}

		[TestMethod()]
		public void LoadTest()
		{
			//Diff.Load(@"d:\daten", @"d:\diff.zip", new Services.Env())
			Program.Main(new string[] { "diff", @"d:\daten", @"d:\diff.zip", "--ignoreDirectories", ".vs", "bin", "obj", "TestResults", });
		}
	}
}