//TODO: if program stopped last saved file is invalid

using CommandLine;
using System;
using System.Threading;
using VersionedCopy;

Parser.Default.ParseArguments<Options>(args).WithParsed(options => Run(options));

void Run(Options options)
{
	var cts = new CancellationTokenSource();
	Console.CancelKeyPress += (_, _) => cts.Cancel();
	Benchmark.Repeat(1, () => Version3MT.Run(options, cts.Token), nameof(Version3MT));
}
