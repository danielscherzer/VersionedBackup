using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using static VersionedCopy.Tests.FileSystemHelper;

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
			Create(src, "F1");
			Create(src, "F2");
			Create(src, "a\\F1");
			Create(src, "b\\");

			Create(dst, "F1");
			Create(dst, "F3");
			Create(dst, "x\\");
			Create(dst, "y\\F3");

			Program.Main(new string[] { "mirror", src, dst });
			var old = GetBackupPath(dst);

			Assert.IsTrue(Directory.EnumerateFiles(old, "F3*").Any());
			Assert.IsTrue(Exists(old, "x\\"));
			Assert.IsTrue(Directory.EnumerateFiles(old, "y\\F3*").Any());

			Assert.IsTrue(Exists(dst, "F1"));
			Assert.IsTrue(Exists(dst, "F2"));
			Assert.IsTrue(Exists(dst, "a\\F1"));
			Assert.IsTrue(Exists(dst, "b\\"));
		}

		[TestMethod()]
		public void MirrorNewerDstTest()
		{
			var src = ToPath("src");
			var dst = ToPath("dst");

			var srcF1 = Create(src, "a\\b\\c\\F1");
			var dstF1 = Create(dst, "a\\b\\c\\F1");
			UpdateWriteTime(dst, "a\\b\\c\\F1", DateTime.Now.AddSeconds(10));

			Program.Main(new string[] { "mirror", src, dst });
			var old = GetBackupPath(dst);

			var newDstF1 = Read(dst, "a\\b\\c\\F1");
			Assert.AreEqual(srcF1, newDstF1);
			var oldF1 = Directory.EnumerateFiles(old, "a\\b\\c\\F1*").First();
			Assert.AreEqual(dstF1, File.ReadAllText(oldF1));
		}

		[TestCleanup]
		public void TestCleanup()
		{
			Cleanup();
		}
	}
}