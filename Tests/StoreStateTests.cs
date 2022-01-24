using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using VersionedCopy.Services;

namespace VersionedCopy.Tests
{
	[TestClass()]
	public class StoreStateTests
	{
		[TestMethod()]
		public void RunTest()
		{
			using Benchmark _ = new("store state");
			StoreState.Run(@"f:\daten", @"d:\test.json", Enumerable.Empty<string>(), Enumerable.Empty<string>());
		}
	}
}