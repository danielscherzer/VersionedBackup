using System;
using System.Diagnostics;

namespace VersionedCopy.Services;

public static class StopwatchExtension
{
	[Conditional("DEBUG")]
	public static void Benchmark(this Stopwatch stopwatch, string message)
	{
		var elapsed = stopwatch.Elapsed;
		stopwatch.Restart();
		Console.WriteLine($"{message} {elapsed.TotalMilliseconds,8:F2}ms");
	}
}
