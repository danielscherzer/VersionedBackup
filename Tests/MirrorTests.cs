using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using static VersionedCopy.Tests.Services.FileSystemHelper;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class MirrorTests
	{
		[TestMethod()]
		public void MirrorTest()
		{
			var src = ToPath("src");
			var dst = ToPath("dst");
			var old = ToPath("old");
			Create(src, "F1");
			Create(src, "F2");
			Create(src, "a", "F1");
			Create(src, "b\\");

			Create(dst, "F1");
			Create(dst, "F3");
			Create(dst, "x\\");
			Create(dst, "y\\F3");

			Program.Main(new string[] { "mirror", src, dst, old });

			Exists(old, "F3");
			Exists(old, "x\\");
			Exists(old, "y\\F3");

			Exists(dst, "F1");
			Exists(dst, "F2");
			Exists(dst, "a", "F1");
			Exists(dst, "b\\");
		}

		[TestMethod()]
		public void MirrorNewerDstTest()
		{
			var src = ToPath("src");
			var dst = ToPath("dst");
			var old = ToPath("old");

			var srcF1 = Create(src, "a\\b\\c\\F1");
			Thread.Sleep(5000);
			var dstF1 = Create(dst, "a\\b\\c\\F1");

			Program.Main(new string[] { "mirror", src, dst, old });

			var newDstF1 = Read(dst, "a\\b\\c\\F1");
			Assert.AreEqual(srcF1, newDstF1);
			var oldF1 = Read(old, "a\\b\\c\\F1");
			Assert.AreEqual(dstF1, oldF1);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			Cleanup();
		}
	}
}