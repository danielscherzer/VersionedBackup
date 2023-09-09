
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static VersionedCopy.Tests.FileSystemHelper;

namespace VersionedCopy.Tests;

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

		foreach (var item in list) Assert.IsTrue(Exists(dst, item));
	}

	[DataTestMethod()]
	[DataRow("update")]
	[DataRow("mirror")]
	[DataRow("sync")]
	public void CopyIgnoreFilesTest(string operation)
	{
		var list = new string[] { "F1", "F2", "a\\F1", "a\\F2", "a\\F3", "a\\F4", "b\\", "c\\", "a\\b\\c\\d" };
		var src = ToPath("src");
		var dst = ToPath("dst");
		var ignore = "F1";
		foreach (var item in list) Create(src, item);

		Program.Main(new string[] { operation, src, dst, "--ignoreFiles", ignore });

		foreach (var item in list)
		{
			if (item.Contains(ignore)) Assert.IsFalse(Exists(dst, item));
			else Assert.IsTrue(Exists(dst, item));
		}
	}

	[DataTestMethod()]
	[DataRow("update")]
	[DataRow("mirror")]
	[DataRow("sync")]
	public void CopyIgnoreDirsTest(string operation)
	{
		var list = new string[] { "F1", "F2", "a\\F1", "a\\F2", "a\\F3", "a\\F4", "b\\", "c\\", "a\\b\\c\\d" };
		var src = ToPath("src");
		var dst = ToPath("dst");
		foreach (var item in list) Create(src, item);

		Program.Main(new string[] { operation, src, dst, "--ignoreDirectories", "a" });

		foreach (var item in list)
		{
			if (item.Contains("a\\")) Assert.IsFalse(Exists(dst, item));
			else Assert.IsTrue(Exists(dst, item));
		}
	}

	[TestCleanup]
	public void TestCleanup()
	{
		Cleanup();
	}
}