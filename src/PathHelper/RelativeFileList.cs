using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VersionedCopy.Interfaces;

namespace VersionedCopy.PathHelper;

public class RelativeFileList : IRelativeFiles
{
	public RelativeFileList(string root, IEnumerable<KeyValuePair<string, DateTime>> items)
	{
		Root = root.IncludeTrailingPathDelimiter();
		Items = items;
	}

	public string FullName(string fileName) => Root + fileName;

	public string Root { get; }
	public IEnumerable<KeyValuePair<string, DateTime>> Items { get; }

	public IEnumerator<KeyValuePair<string, DateTime>> GetEnumerator() => Items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

	public override string ToString() => $"{Items.Count()}";
}
