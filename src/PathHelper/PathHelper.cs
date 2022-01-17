using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VersionedCopy.PathHelper
{
	public static class PathHelper
	{
		public static IEnumerable<string> Ignore(this IEnumerable<string> paths, IEnumerable<string> ignorePaths)
		{
			var regexIgnorePaths = ignorePaths.Select(ignorePath
				=> new Regex(ignorePath.WildcardToRegex(), RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled))
				.ToList();

			return paths.Where(path => !regexIgnorePaths.Any(regex => regex.IsMatch(path)));
		}

		public static string IncludeTrailingPathDelimiter(this string path) => Path.EndsInDirectorySeparator(path) ? path : path + Path.DirectorySeparatorChar;

		public static string NormalizePathDelimiter(this string path) => path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

		public static IEnumerable<string> ToRelative(this IEnumerable<string> paths, string prefix)
			=> paths.Select(path => path[prefix.Length..]);

		public static string WildcardToRegex(this string pattern)
		{
			var directorySeparator = Regex.Escape(Path.DirectorySeparatorChar.ToString());
			var exceptDirectorySeparator = $"[^{directorySeparator}]";
			var hasWildcard = pattern.Contains('*') || pattern.Contains('?');
			var noWildcard = !hasWildcard ? directorySeparator : "";
			var regexMiddle = Regex.Escape(pattern)
				.Replace("\\*", exceptDirectorySeparator + '*')
				.Replace("\\?", exceptDirectorySeparator);
			var startAnchor = pattern.StartsWith(Path.DirectorySeparatorChar) ? "^" : "";
			var endAnchor = pattern.EndsWith(Path.DirectorySeparatorChar) ? "" : "$";
			return $"{startAnchor}{noWildcard}{regexMiddle}{endAnchor}";
		}
	}
}
