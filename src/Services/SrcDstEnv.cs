using System.Threading;
using VersionedCopy.Interfaces;

namespace VersionedCopy.Services
{
	public class SrcDstEnv : Env
	{
		public SrcDstEnv(ISrcDstOptions options, IOutput output, CancellationToken token) : base(options, output, token)
		{
			Options = options;
			EnvExtensions.Setup(options.SourceDirectory, options.DestinationDirectory, output, options.ReadOnly, FileSystem);
		}

		public ISrcDstOptions Options { get; }
	}
}
