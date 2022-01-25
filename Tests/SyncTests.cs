using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using static VersionedCopy.Tests.Services.FileSystemHelper;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class SyncTests
	{
		[TestMethod()]
		public void SyncTest()
		{
			var src = Path.Combine(Root, "src");
			var dst = Path.Combine(Root, "dst");
			Create(src, "F1");
			Create(src, "F2");
			Create(src, "a", "F1");
			Create(src, "b\\");

			Create(dst, "F1");
			Create(dst, "F3");
			Create(dst, "x\\");

			Program.Main(new string[] { "sync", src, dst });

			Exists(src, "F3");
			Exists(src, "x\\");

			Exists(dst, "F2");
			Exists(dst, "a", "F1");
			Exists(dst, "b\\");
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (Directory.Exists(Root)) Directory.Delete(Root, true);
		}
	}
}