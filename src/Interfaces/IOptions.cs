using System.Collections.Generic;

namespace VersionedCopy.Interfaces;

public interface IOptions
{
	IEnumerable<string> IgnoreDirectories { get; }
	IEnumerable<string> IgnoreFiles { get; }
	bool ReadOnly { get; }
}