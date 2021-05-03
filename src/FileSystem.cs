using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VersionedCopy
{
	internal static class FileSystem
	{
		internal static IEnumerable<string> EnumerateDirsRecursive(this string dir)
		{
			var stack = new Stack<string>();
			stack.Push(dir);
			yield return dir;
			while (stack.Count > 0)
			{
				foreach (var subDir in Directory.EnumerateDirectories(stack.Pop()))
				{
					yield return subDir;
					stack.Push(subDir);
				}
			}
		}

		internal static IEnumerable<string> EnumerateFiles(this IEnumerable<string> dirs) =>
			from subDir in dirs
			from file in Directory.EnumerateFiles(subDir)
			select file;

		internal static string IncludeTrailingPathDelimiter(this string path) => Path.EndsInDirectorySeparator(path) ? path : path + Path.DirectorySeparatorChar;

		internal static void Move(string source, string destination)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? destination);
			Directory.Move(source, destination);
			//Console.WriteLine($"Moved '{src}'");
		}
	}
}