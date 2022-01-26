using Microsoft.VisualStudio.TestTools.UnitTesting;
using static VersionedCopy.Tests.FileSystemHelper;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class SyncTests
	{
		[TestMethod()]
		public void SyncTest()
		{
			var src = ToPath("src");
			var dst = ToPath("dst");
			Create(src, "F1");
			Create(src, "F2");
			Create(src, "a\\F1");
			Create(src, "b\\");

			Create(dst, "F1");
			Create(dst, "F3");
			Create(dst, "x\\");

			Program.Main(new string[] { "sync", src, dst });

			Assert.IsTrue(Exists(src, "F3"));
			Assert.IsTrue(Exists(src, "x\\"));

			Assert.IsTrue(Exists(dst, "F2"));
			Assert.IsTrue(Exists(dst, "a\\F1"));
			Assert.IsTrue(Exists(dst, "b\\"));
		}

		[TestCleanup]
		public void TestCleanup()
		{
			Cleanup();
		}
	}
}