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
			return Directory.EnumerateDirectories(dir, "*.*", SearchOption.AllDirectories);
			//var stack = new Stack<string>();
			//stack.Push(dir);
			//yield return dir;
			//while (stack.Count > 0)
			//{
			//	foreach (var subDir in Directory.EnumerateDirectories(stack.Pop()))
			//	{
			//		yield return subDir;
			//		stack.Push(subDir);
			//	}
			//}
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