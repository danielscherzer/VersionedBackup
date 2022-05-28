using System;
using System.Collections.Generic;

namespace VersionedCopy.Interfaces
{
	public interface IRelativeFiles : IEnumerable<KeyValuePair<string, DateTime>>
	{
		string Root { get; }
	}
}
