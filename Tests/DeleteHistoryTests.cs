using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class DeleteHistoryTests
	{
		[TestMethod()]
		public void UpdateDeleteHistoryTest()
		{
			Snapshot old = new(DateTime.UtcNow.AddMinutes(-5));
			old.AddFile("a", old.TimeStamp, old.TimeStamp);
			old.AddFile("b", old.TimeStamp, old.TimeStamp);
			old.Directories.Add("d\\", old.TimeStamp);
			old.Directories.Add("d1\\", old.TimeStamp);
			Snapshot current = new();
			current.AddFile("a", current.TimeStamp, current.TimeStamp);
			current.Directories.Add("d\\", current.TimeStamp);
			DeleteHistory history = new();
			history.Update(old, current);
			CollectionAssert.AreEquivalent(new string[] { "b", "d1\\" } ,history.Keys);
		}
	}
}