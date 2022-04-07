using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static VersionedCopy.Tests.FileSystemHelper;

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

			Assert.IsTrue(Exists(dst, "F1"));
			Assert.IsTrue(Exists(dst, "F3"));
			Assert.IsTrue(Exists(dst, "x\\"));

			Assert.IsTrue(Exists(dst, "F2"));
			Assert.IsTrue(Exists(dst, "a\\F1"));
			Assert.IsTrue(Exists(dst, "b\\"));
		}

		[TestMethod()]
		public void UpdateNewerSrcTest()
		{
			var src = ToPath("src");
			var dst = ToPath("dst");
			Create(dst, "a\\b\\c\\F1");

			var srcF1 = Create(src, "a\\b\\c\\F1");
			UpdateWriteTime(src, "a\\b\\c\\F1", DateTime.Now.AddSeconds(5));


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


			var dstF1 = Create(dst, "a\\b\\c\\F1");
			UpdateWriteTime(dst, "a\\b\\c\\F1", DateTime.Now.AddSeconds(5));

			Program.Main(new string[] { "update", src, dst });

			var newDstF1 = Read(dst, "a\\b\\c\\F1");
			Assert.AreEqual(dstF1, newDstF1);
		}

		[TestCleanup]
		public void TestCleanup() => Cleanup();
	}
}