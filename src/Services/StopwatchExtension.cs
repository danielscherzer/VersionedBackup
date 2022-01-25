using System.Diagnostics;

namespace VersionedCopy.Services
{
	public static class StopwatchExtension
	{
		public static void Benchmark(this Stopwatch stopwatch, string message)
		{
			var elapsed = stopwatch.Elapsed;
			stopwatch.Restart();
			Trace.WriteLine($"{elapsed.TotalMilliseconds,8:F2}ms {message}");
		}
	}
}
