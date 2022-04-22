using System;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VersionedCopy.PathHelper;
using VersionedCopy.Services;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace VersionedCopy
{
	public static class Diff
	{
		static readonly byte[] key =
{
				0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
				0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
			};
		public static void AddEntry(this ZipArchive zip, string key, string fileName)
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
			using var aes = Aes.Create();
			stream.Read(aes.IV, 0, aes.IV.Length);
			aes.Key = key;
			using var cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read);
			using var zip = new ZipArchive(cryptoStream, ZipArchiveMode.Read, false, Encoding.UTF8);
			foreach(var entry in zip.Entries)
			{

			}
		}
		public static void Save(string directory, string dstFileName, Env env)
		{
			Console.WriteLine($"Store diff from '{directory}' to '{dstFileName}'");
			// Create a snapshot from source
			var taskSnap = Task.Run(() => env.CreateSnapshot(directory));
			//TODO: load old snapshot from db( from where?) because we need our old snapshot from the previous diff and not sync or other ops
			var taskSnapOld = Task.Run(() => Snapshot.Load(directory));
			Task.WaitAll(new Task[] { taskSnap, taskSnapOld }, cancellationToken: env.Token);
			if (env.Token.IsCancellationRequested) return;

			var snap = taskSnap.Result;
			// old snapshot must be present
			if (taskSnapOld.Result is Snapshot snapOld)
			{
				var newFiles = snap.Singles(snapOld);
				var delFiles = snapOld.Singles(snap);
				snap.FindUpdatedFiles(snapOld, out var changedFiles, out var replacedFiles);

				if (!newFiles.Any() && !delFiles.Any() && !changedFiles.Any() && !replacedFiles.Any())
				{
					Console.WriteLine($"No change in '{directory}'");
					return;
				}

				using var stream = File.Create(dstFileName);
				
				//using var aes = Aes.Create();
				//stream.Write(aes.IV, 0, aes.IV.Length);
				//aes.Key = key;
				//using var cryptoStream = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write);

				using var zip = new ZipArchive(stream, ZipArchiveMode.Create, false, Encoding.UTF8);
				//store singles and changed files
				foreach (var file in newFiles.Concat(changedFiles).Concat(replacedFiles))
				{
					zip.AddEntry(file.Key, snap.FullName(file.Key));
				}
				//store list of deleted in file in zip
				if (delFiles.Any())
				{
					var json = JsonConvert.SerializeObject(delFiles, Formatting.Indented);
					var entry = zip.CreateEntry(".diff-deletedFiles");
					using StreamWriter writer = new(entry.Open());
					writer.Write(json);
				}
			}
		}
	}
}
