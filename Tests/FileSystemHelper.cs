using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Tests
{
	internal static class FileSystemHelper
	{
		private static readonly string Root = Path.Combine(Path.GetTempPath(), "VersionedCopy");

		public static string ToPath(params string[] nameParts) => Path.Combine(Root, Path.Join(nameParts));
		
		public static string GetBackupPath(string path)
		{
			return Snapshot.GetMetaDataDir(path);
		}

		public static void Cleanup()
		{
			if (Directory.Exists(Root)) Directory.Delete(Root, true);
		}

		public static string Create(string path1, string path2)
		{
			var path = ToPath(path1, path2);
			if (Path.EndsInDirectorySeparator(path))
			{
				Directory.CreateDirectory(path);
				return "";
			}
			else
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllText(path, path);
				return path;
			}
		}

		public static bool Exists(string path1, string path2)
		{
			var path = ToPath(path1, path2);
			if (Path.EndsInDirectorySeparator(path))
			{
				return Directory.Exists(path);
			}
			else
			{
				return File.Exists(path);
			}
		}

		public static IEnumerable<string> SearchFiles(string path1, string path2)
		{
			return Directory.EnumerateFiles(path1, path2);
		}

		public static string Read(string path1, string path2)
		{
			var path = ToPath(path1, path2);
			return File.ReadAllText(path);
		}

		public static void UpdateWriteTime(string path1, string path2, DateTime dateTime)
		{
			var path = ToPath(path1, path2);
			File.SetLastWriteTimeUtc(path, dateTime);
		}
	}
}
