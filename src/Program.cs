using CommandLine;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading;
using VersionedCopy.Interfaces;
using VersionedCopy.Options;
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
				output.Error("CANCEL received - stopping operations!");
				cts.Cancel();
				args.Cancel = true; // means to continue the process!, no hard cancel, but give process time to cleanup
			};

			Parse(args)
				.WithParsed<MirrorOptions>(options => Mirror.Run(new AlgorithmEnv(options, output, cts.Token)))
				.WithParsed<UpdateOptions>(options => Update.Run(new AlgorithmEnv(options, output, cts.Token)))
				.WithParsed<SyncOptions>(options => Sync.Run(new AlgorithmEnv(options, output, cts.Token)))
				.WithParsed<DiffOptions>(options => Diff.Save(options.Directory, options.FileName, new Env(options, output, cts.Token)))
				.WithParsed<AssemblyUpdateOptions>(options => AssemblyUpdate.Update());
#if DEBUG
			stopwatch.Benchmark("Total");
#endif
		}

		public static ParserResult<object> Parse(string[] args)
		{
			var parser = new Parser(with => { with.CaseSensitive = false; with.AutoHelp = true; with.HelpWriter = Console.Error; });
			var result = parser.ParseArguments<MirrorOptions, UpdateOptions, DiffOptions, SyncOptions, AssemblyUpdateOptions>(args);
			return result;
		}
	}
}