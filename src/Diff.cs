using System;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;
using System.Linq;
using System.Security.Cryptography;

namespace VersionedCopy
{
	public static class Diff
	{
		public static void AddEntry(this ZipArchive zip, string fileName, string key)
		{
			if (Snapshot.IsFile(fileName))
			{
				key = key.Replace('\\', '/');
				zip.CreateEntryFromFile(fileName, key);
			}
			else
			{
				key = key.Replace('\\', '/');
				zip.CreateEntry(key);
			}
		}

		public static void Load(string directory, string diffFileName, Env env)
		{
			Console.WriteLine($"Load diff from '{diffFileName}' to '{directory}'");
			using var stream = File.OpenRead(diffFileName);
			using var zip = new ZipArchive(stream, ZipArchiveMode.Create, false, Encoding.UTF8);
			foreach(var entry in zip.Entries)
			{

			}
		}
		public static void Save(string directory, string dstFileName, Env env)
		{
			Console.WriteLine($"Store diff from '{directory}' to '{dstFileName}'");
			// Create a snapshot from source
			var taskSnap = Task.Run(() => Snapshot.Create(directory, env.IgnoreDirectories, env.IgnoreFiles, env.Token));
			// load old snapshot from source
			var taskSnapOld = Task.Run(() => Snapshot.Load(directory));
			Task.WaitAll(new Task[] { taskSnap, taskSnapOld }, cancellationToken: env.Token);
			if (env.Token.IsCancellationRequested) return;

			var snap = taskSnap.Result;
			// old snapshot must be present
			if (taskSnapOld.Result is Snapshot snapOld)
			{
				using var stream = File.Create(dstFileName);

				//using var provider = new AesCryptoServiceProvider();
				//stream.Write(provider.IV, 0, provider.IV.Length);
				//using var cryptoTransform = provider.CreateEncryptor();
				//using var cryptoStream = new CryptoStream(stream, cryptoTransform, CryptoStreamMode.Write);

				var singles = snap.Singles(snapOld);
				snap.FindUpdatedFiles(snapOld, out var changedFiles, out var replacedFiles);

				using var zip = new ZipArchive(stream, ZipArchiveMode.Create, false, Encoding.UTF8);
				//store singles and changed files
				foreach (var file in singles.Concat(changedFiles).Concat(replacedFiles))
				{
					zip.AddEntry(snap.FullName(file.Key), file.Key);
				}
			}
		}
	}
}
