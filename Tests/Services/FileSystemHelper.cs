using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace VersionedCopy.Tests.Services
{
	internal static class FileSystemHelper
	{
		private static readonly string Root = Path.Combine(Path.GetTempPath(), "VersionedCopy");
		public static string ToPath(params string[] nameParts) => Path.Combine(Root, Path.Join(nameParts));
		public static void Cleanup()
		{
			if (Directory.Exists(Root)) Directory.Delete(Root, true);
		}


		public static string Create(params string[] nameParts)
		{
			var path = ToPath(nameParts);
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

		public static void Exists(params string[] nameParts)
		{
			var path = ToPath(nameParts);
			if (Path.EndsInDirectorySeparator(path))
			{
				Assert.IsTrue(Directory.Exists(path));
			}
			else
			{
				Assert.IsTrue(File.Exists(path));
			}
		}

		public static string Read(params string[] nameParts)
		{
			var path = ToPath(nameParts);
			return File.ReadAllText(path);
		}
	}
}
