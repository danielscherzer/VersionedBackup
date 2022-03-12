using System.Collections.Generic;
using System;

namespace VersionedCopy.Interfaces
{
	public interface IRelativeFiles : IEnumerable<KeyValuePair<string, DateTime>>
	{
		string Root { get; }
	}
}
