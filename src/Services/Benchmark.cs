using System;
using System.Diagnostics;

namespace VersionedCopy.Services
{
	class Benchmark : IDisposable
	{
		public Benchmark(Action<string> print, string message = "")
		{
			Print = print ?? throw new ArgumentNullException(nameof(print));
			Message = message;
		}

		public Benchmark(string message = "")
		{
			Print = message => Console.WriteLine(message);
			Message = message;
		}

		public Action<string> Print { get; }
		public string Message { get; set; }

		public void Dispose()
		{
			var time = _stopwatch.Elapsed;
			_stopwatch.Stop();
			Output(Message, time);
		}

		private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

		private void Output(string message, TimeSpan elapsed) => Print($"{elapsed.TotalMilliseconds,8:F2}ms {message}");
	}
}
