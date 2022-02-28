using CommandLine;
using System;
using System.Diagnostics;
using System.Threading;
using VersionedCopy.Interfaces;
using VersionedCopy.Services;

namespace VersionedCopy
{
	public static class Program
	{
		private static void Run(IOptions options, Output report, CancellationToken token, Action<AlgorithmEnv> algo)
		{
			if (options.SourceDirectory == options.DestinationDirectory) throw new ArgumentException("Source and destination must be different!");
#if DEBUG
			Stopwatch stopwatch = Stopwatch.StartNew();
#endif
			var fileSystem = new FileSystem(report, options.DryRun);
			if (!fileSystem.ExistsDirectory(options.SourceDirectory))
			{
				report.Error($"Source directory '{options.SourceDirectory}' does not exist");
				return;
			}
			algo(new AlgorithmEnv(options, report, fileSystem, token));
#if DEBUG
			stopwatch.Benchmark("Copy");
#endif
		}

		public static void Main(string[] args)
		{
			// create logger service
			Output report = new();
			using CancellationTokenSource cts = new();
			Console.CancelKeyPress += (_, args) =>
			{
				report.Error("CANCEL received - stopping opperations!");
				cts.Cancel();
				args.Cancel = true; // means to continue the process!, no hard cancel, but give process time to cleanup
			};

			//ServiceLocator.AddService<IFileSystem>(new FileSystem(report, options.DryRun));
			var parser = new Parser(with => { with.CaseSensitive = false; with.AutoHelp = true; with.HelpWriter = Console.Error; });
			var result = parser.ParseArguments<MirrorOptions, UpdateOptions, SyncOptions, SnapshotOptions, AssemblyUpdateOptions>(args)
				.WithParsed<MirrorOptions>(options => Run(options, report, cts.Token, Mirror.Run))
				.WithParsed<UpdateOptions>(options => Run(options, report, cts.Token, Update.Run))
				.WithParsed<SyncOptions>(options => Run(options, report, cts.Token, Sync.Run))
				.WithParsed<SnapshotOptions>(options => Snapshot.Run(options.Directory, options.DatabaseFileName, options.IgnoreDirectories, options.IgnoreFiles, cts.Token))
				.WithParsed<AssemblyUpdateOptions>(options => AssemblyUpdate.Update());
		}
	}
}