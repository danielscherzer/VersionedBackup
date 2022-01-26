using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using static VersionedCopy.Tests.Services.FileSystemHelper;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class UpdateTests
	{
		[TestMethod()]
		public void UpdateTest()
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

			Program.Main(new string[] { "update", src, dst });

			Exists(dst, "F1");
			Exists(dst, "F3");
			Exists(dst, "x\\");

			Exists(dst, "F2");
			Exists(dst, "a\\F1");
			Exists(dst, "b\\");
		}

		[TestMethod()]
		public void UpdateNewerSrcTest()
		{
			var src = ToPath("src");
			var dst = ToPath("dst");
			Create(dst, "a\\b\\c\\F1");
			Thread.Sleep(5000);
			var srcF1 = Create(src, "a\\b\\c\\F1");

			Program.Main(new string[] { "update", src, dst });

			var newDstF1 = Read(dst, "a\\b\\c\\F1");
			Assert.AreEqual(srcF1, newDstF1);
		}

		[TestMethod()]
		public void UpdateNewerDstTest()
		{
			var src = ToPath("src");
			var dst = ToPath("dst");
			Create(src, "a\\b\\c\\F1");
			Thread.Sleep(5000);
			var dstF1 = Create(dst, "a\\b\\c\\F1");

			Program.Main(new string[] { "update", src, dst });

			var newDstF1 = Read(dst, "a\\b\\c\\F1");
			Assert.AreEqual(dstF1, newDstF1);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			Cleanup();
		}
	}
}