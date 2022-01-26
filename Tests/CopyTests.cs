using Microsoft.VisualStudio.TestTools.UnitTesting;
using static VersionedCopy.Tests.Services.FileSystemHelper;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class CopyTests
	{
		[DataTestMethod()]
		[DataRow("update")]
		[DataRow("mirror")]
		[DataRow("sync")]
		public void CopyTest(string operation)
		{
			var list = new string[] { "F1", "F2", "a\\F1", "a\\F2", "a\\F3", "a\\F4", "b\\", "c\\", "a\\b\\c\\d" };
			var src = ToPath("src");
			var dst = ToPath("dst");
			foreach (var item in list) Create(src, item);

			Program.Main(new string[] { operation, src, dst });

			foreach (var item in list) Exists(dst, item);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			Cleanup();
		}
	}
}