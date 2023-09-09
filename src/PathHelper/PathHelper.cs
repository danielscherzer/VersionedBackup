using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace VersionedCopy.PathHelper;

public static class PathHelper
{
	public static Regex CreateIgnoreRegex(this string ignorePath) => new(ignorePath.WildcardToRegex(), RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

	public static IEnumerable<Regex> CreateIgnoreRegex(this IEnumerable<string> ignorePaths) => ignorePaths.Select(ignorePath => CreateIgnoreRegex(ignorePath));

	public static bool AnyMatch(this IEnumerable<Regex> regex, string input) => regex.Any(regex => regex.IsMatch(input));

	public static string IncludeTrailingPathDelimiter(this string path) => Path.EndsInDirectorySeparator(path) ? path : path + Path.DirectorySeparatorChar;

	public static string NormalizePathDelimiter(this string path) => path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

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
