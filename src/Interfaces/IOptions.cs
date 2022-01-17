using System.Collections.Generic;

namespace VersionedCopy.Interfaces
{
	public interface IOptions : IDirectories
	{
		IEnumerable<string> IgnoreDirectories { get; }
		IEnumerable<string> IgnoreFiles { get; }
		bool DryRun { get; }
		public AlgoMode Mode { get; }
	}
}