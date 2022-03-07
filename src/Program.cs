using CommandLine;
using System;
using System.Diagnostics;
using System.Threading;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public static class Program
	{
		public static void Main(string[] args)
		{
#if DEBUG
			Stopwatch stopwatch = Stopwatch.StartNew();
#endif
			// create logger service
			Output output = new();
			using CancellationTokenSource cts = new();
			Console.CancelKeyPress += (_, args) =>
			{
				output.Error("CANCEL received - stopping opperations!");
				cts.Cancel();
				args.Cancel = true; // means to continue the process!, no hard cancel, but give process time to cleanup
			};

			var parser = new Parser(with => { with.CaseSensitive = false; with.AutoHelp = true; with.HelpWriter = Console.Error; });
			var result = parser.ParseArguments<MirrorOptions, UpdateOptions, SyncOptions, SnapshotOptions, AssemblyUpdateOptions>(args)
				.WithParsed<MirrorOptions>(options => Mirror.Run(new AlgorithmEnv(options, output, cts.Token)))
				.WithParsed<UpdateOptions>(options => Update.Run(new AlgorithmEnv(options, output, cts.Token)))
				.WithParsed<SyncOptions>(options => Sync.Run(new AlgorithmEnv(options, output, cts.Token)))
				.WithParsed<SnapshotOptions>(options => Snapshot.Run(options.Directory, options.DatabaseFileName, options.IgnoreDirectories, options.IgnoreFiles, cts.Token))
				.WithParsed<AssemblyUpdateOptions>(options => AssemblyUpdate.Update());
#if DEBUG
			stopwatch.Benchmark("Total");
#endif
		}
	}
}