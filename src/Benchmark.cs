using System;
using System.Diagnostics;

namespace VersionedCopy
{
	class Benchmark
	{
		readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		TimeSpan last = TimeSpan.Zero;

		internal void Delta(string message = "")
		{
			var elapsed = _stopwatch.Elapsed;
			Print(message, elapsed - last);
			last = elapsed;
		}

		internal static void Repeat(int count, Action action)
		{
			for (int i = 0; i < count; ++i) action();
		}

		internal void Reset(string message = "")
		{
			_stopwatch.Restart();
			last = TimeSpan.Zero;
			Log.Print($"{message}");
		}

		internal void Total(string message = "")
		{
			var elapsed = _stopwatch.Elapsed;
			Print(message + "(total)", elapsed);
			last = elapsed;
		}

		private static void Print(string message, TimeSpan elapsed)
		{
			Log.Print($"{(elapsed).TotalMilliseconds, 8:F2}ms {message}");
		}
	}
}
