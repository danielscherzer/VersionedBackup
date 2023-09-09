using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace VersionedCopy.PathHelper;

public interface INamed
{
	public string Name { get; }
}

public class NamedComparer : IEqualityComparer<INamed>
{
	public bool Equals(INamed? x, INamed? y)
	{
		if (x != null)
		{
			return x.Equals(y);
		}
		else
		{
			return y == null;
		}
	}

	public int GetHashCode([DisallowNull] INamed obj) => obj.GetHashCode();

	public static readonly NamedComparer Singleton = new();
}

public class FileState : INamed
{
	public FileState(string name, DateTime lastWriteTime)
	{
		Name = name;
		LastWriteTime = lastWriteTime;
	}

	public string Name { get; }
	public DateTime LastWriteTime { get; }
	public override string ToString() => Name;
}

public class DirectoryState : INamed
{
	public DirectoryState(string name)
	{
		Name = name;
	}

	public string Name { get; }
	public HashSet<DirectoryState> Directories { get; } = new(NamedComparer.Singleton);
	public HashSet<FileState> Files { get; } = new(NamedComparer.Singleton);
	public override string ToString() => Name;

	public static DirectoryState Create(string directory, IEnumerable<string> ignoreDirectories, IEnumerable<string> ignoreFiles, System.Threading.CancellationToken cancellationToken)
	{
		var root = new DirectoryInfo(directory);
		if (!root.Exists) throw new DirectoryNotFoundException(directory);
		var regexIgnoreDirectories = ignoreDirectories.CreateIgnoreRegex().ToList();
		var regexIgnoreFiles = ignoreFiles.CreateIgnoreRegex().ToList();

		var rootState = new DirectoryState(root.Name);

		var subDirs = new Stack<(DirectoryInfo, DirectoryState)>();
		subDirs.Push((root, rootState));
		while (subDirs.Count > 0)
		{
			(DirectoryInfo dir, DirectoryState dirState) = subDirs.Pop();
			try
			{
				foreach (var file in dir.EnumerateFiles())
				{
					if (cancellationToken.IsCancellationRequested) return rootState;
					if (regexIgnoreFiles.AnyMatch(file.FullName)) continue;
					dirState.Files.Add(new FileState(file.Name, file.LastWriteTimeUtc));
				}
				foreach (var subDir in dir.EnumerateDirectories())
				{
					if (cancellationToken.IsCancellationRequested) return rootState;
					if (regexIgnoreDirectories.AnyMatch(subDir.FullName + Path.DirectorySeparatorChar)) continue;
					var subDirState = new DirectoryState(subDir.Name);
					dirState.Directories.Add(subDirState);
					subDirs.Push((subDir, subDirState));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
		}
		return rootState;
	}
}
