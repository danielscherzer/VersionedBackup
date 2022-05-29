using System.IO;
using System.Threading;
using VersionedCopy.Interfaces;
using VersionedCopy.PathHelper;

namespace VersionedCopy.Services
{
	public class SrcDstEnv : Env
	{
		public SrcDstEnv(ISrcDstOptions options, Output output, CancellationToken token) : base(options, output, token)
		{
			Options = options;
			EnvExtensions.Setup(options.SourceDirectory, options.DestinationDirectory, output, options.ReadOnly, FileSystem);
			output.SetLogFile(Path.Combine(Snapshot.GetMetaDataDir(options.SourceDirectory), "operations.log"));
		}

		public ISrcDstOptions Options { get; }
	}
}
