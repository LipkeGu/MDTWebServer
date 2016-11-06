﻿using System;
using System.IO;

namespace MDTWebService
{
	public static class Filesystem
	{
		public static long Size(string filename)
		{
			var size = 0L;
			using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
				size = fs.Length;

			return size;
		}

		public static bool Exist(string filename) => File.Exists(filename.ToLowerInvariant());

		public static string ResolvePath(string path)
		{
			return Path.Combine(Environment.CurrentDirectory, ReplaceSlashes(path.ToLowerInvariant()));
		}

		public static string StripRoot(string path, string directory) => path.Replace(directory, string.Empty);

		public static string ReplaceSlashes(string input)
		{
			var slash = "/";
			var result = string.Empty;

			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.MacOSX:
				case PlatformID.Unix:
					slash = "/";
					break;
				case PlatformID.Win32NT:
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.WinCE:
				case PlatformID.Xbox:
					slash = "\\";
					break;
			}

			result = input.Replace("/", slash);
			result = result.Replace("\\", slash);

			return result;
		}
	}
}
