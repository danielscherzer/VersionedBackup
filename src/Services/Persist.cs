﻿using Newtonsoft.Json;
using System.IO;

namespace VersionedCopy.Services
{
	internal static class Persist
	{
		public static void Save(this object obj, string fileName)
		{
			var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
			File.WriteAllText(fileName, json);
		}

		internal static TData? Load<TData>(string fileName)
		{
			if (!File.Exists(fileName)) return default;
			var json = File.ReadAllText(fileName);
			return JsonConvert.DeserializeObject<TData>(json);
		}
	}
}