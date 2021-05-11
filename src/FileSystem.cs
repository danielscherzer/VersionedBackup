using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VersionedCopy
{
	internal static class FileSystem
	{
		//TODO: make multi threaded
		internal static IEnumerable<string> EnumerateDirsRecursive(this string dir)
		{
			var stack = new Stack<string>();
			stack.Push(dir);
			yield return dir;
			while (stack.Count > 0)
			{
				foreach (var subDir in Directory.EnumerateDirectories(stack.Pop()))
				{
					yield return subDir + Path.DirectorySeparatorChar;
					stack.Push(subDir);
				}
			}
		}

		internal static void Copy(string srcFilePath, string dstFilePath)
		{
			LogCatch(() =>
			{
				File.Copy(srcFilePath, dstFilePath);
				Log.Print($"Copy file '{srcFilePath}' => {dstFilePath}");
			});
		}

		internal static void CreateDirectory(string path) => LogCatch(() => Directory.CreateDirectory(path));

		internal static IEnumerable<string> EnumerateFiles(this IEnumerable<string> dirs) =>
			from subDir in dirs.AsParallel()
			from file in Directory.EnumerateFiles(subDir)
			select file;

		internal static FileInfo? GetFileInfo(string path)
		{
			try
			{
				return new FileInfo(path);
			}
			catch(SystemException e)
			{
				Log.Print(e.Message);
				return null;
			}
		}

		internal static IEnumerable<string> Ignore(this IEnumerable<string> paths, IEnumerable<string> ignorePaths)
		{
			List<string> absoluteIgnorePaths = new();
			List<string> relativeIgnorePaths = new();
			foreach (var ignorePath in ignorePaths)
			{
				if (ignorePath.StartsWith(Path.DirectorySeparatorChar))
				{
					absoluteIgnorePaths.Add(ignorePath.IncludeTrailingPathDelimiter());
				}
				else
				{
					relativeIgnorePaths.Add((Path.DirectorySeparatorChar + ignorePath).IncludeTrailingPathDelimiter());
				}
			}

			bool Accept(string path)
			{
				foreach (var ignorePath in absoluteIgnorePaths)
				{
					if (path.StartsWith(ignorePath)) return false;
				}
				foreach (var ignorePath in relativeIgnorePaths)
				{
					if (path.Contains(ignorePath)) return false;
				}
				return true;
			}
			return paths.Where(path => Accept(path));
		}

		internal static string IncludeTrailingPathDelimiter(this string path) => Path.EndsInDirectorySeparator(path) ? path : path + Path.DirectorySeparatorChar;

		internal static void Move(string source, string destination)
		{
			LogCatch(() =>
			{
				Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? destination);
				Directory.Move(source, destination);
				Log.Print($"Moving '{source}' => {destination}");
			});
		}

		internal static HashSet<string> ToRelative(this IEnumerable<string> paths, string prefix)
	=> paths.Select(path => path[prefix.Length..]).ToHashSet();

		private static void LogCatch(Action action)
		{
			try
			{
				action();
			}
			catch (IOException e)
			{
				Log.Print(e.Message);
			}
		}
	}
}