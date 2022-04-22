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
			var listSrc = new string[] { "F1", "F2", "a\\F1", "a\\F2", "a\\F3", "a\\F4", "b\\", "c\\", "a\\b\\c\\d" };
			foreach (var item in listSrc) Create(src, item);

			var listDst = new string[] { "F1", "F3", "x\\", "x\\F2" };
			foreach (var item in listDst) Create(dst, item);
			Create(dst, "F1");
			Create(dst, "F3");
			Create(dst, "x\\");

			Program.Main(new string[] { "sync", src, dst });

			foreach (var item in listSrc) Assert.IsTrue(Exists(dst, item));
			foreach (var item in listDst) Assert.IsTrue(Exists(src, item));
		}

		[TestCleanup]
		public void TestCleanup() => Cleanup();
	}
}