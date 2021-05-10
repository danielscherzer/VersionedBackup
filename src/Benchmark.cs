using System;
using System.Diagnostics;

namespace VersionedCopy
{
	class Benchmark : IDisposable
	{
		public Benchmark(string message = "")
		{
			Message = message;
		}

		public string Message { get; set; }

		public void Dispose()
		{
			var time = _stopwatch.Elapsed;
			_stopwatch.Stop();
			Print(Message, time);
		}

		public static void Repeat(int count, Action action, string name = "")
		{
			for (int i = 0; i < count; ++i)
			{
				using var _ = new Benchmark(name);
				action();
			}
		}

		private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

		private static void Print(string message, TimeSpan elapsed) => Log.Print($"{Conv(elapsed.TotalMilliseconds)} {message}");

		private static string Conv(double msec) => $"{msec,8:F2}ms";
	}
}
