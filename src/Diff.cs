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
		static byte[] key =
{
				0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
				0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
			};
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
			// load old snapshot from source
			var taskSnapOld = Task.Run(() => Snapshot.Load(directory));
			Task.WaitAll(new Task[] { taskSnap, taskSnapOld }, cancellationToken: env.Token);
			if (env.Token.IsCancellationRequested) return;

			var snap = taskSnap.Result;
			// old snapshot must be present
			if (taskSnapOld.Result is Snapshot snapOld)
			{
				using var stream = File.Create(dstFileName);
				
				using var aes = Aes.Create();
				stream.Write(aes.IV, 0, aes.IV.Length);
				aes.Key = key;
				using var cryptoStream = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write);

				var singles = snap.Singles(snapOld);
				snap.FindUpdatedFiles(snapOld, out var changedFiles, out var replacedFiles);

				using var zip = new ZipArchive(cryptoStream, ZipArchiveMode.Create, false, Encoding.UTF8);
				//store singles and changed files
				foreach (var file in singles.Concat(changedFiles).Concat(replacedFiles))
				{
					zip.AddEntry(snap.FullName(file.Key), file.Key);
				}
			}
		}
	}
}
