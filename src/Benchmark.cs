using System;
using System.Diagnostics;
using System.Text;

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

		internal void Repeat(int count, Action action, string name = "")
		{
			StringBuilder sb = new(name);
			sb.Append(" ");
			for (int i = 0; i < count; ++i)
			{
				Reset();
				action();
				sb.Append(Conv(_stopwatch.Elapsed.TotalMilliseconds));
			}
			Log.Print(sb.ToString());
		}

		internal void Reset(string message = "")
		{
			_stopwatch.Restart();
			last = TimeSpan.Zero;
			if(!string.IsNullOrEmpty(message)) Log.Print(message);
		}

		internal void Total(string message = "")
		{
			var elapsed = _stopwatch.Elapsed;
			Print(message + "(total)", elapsed);
			last = elapsed;
		}

		private static void Print(string message, TimeSpan elapsed)
		{
			Log.Print($"{Conv(elapsed.TotalMilliseconds)} {message}");
		}

		private static string Conv(double msec) => $"{msec,8:F2}ms";
	}
}
