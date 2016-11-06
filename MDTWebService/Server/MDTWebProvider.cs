using System;
using System.IO;
using System.Net;
using MDTWebService.Klassen;

namespace MDTWebService
{
	public static class MDTWebProvider
	{
		public static string SendResponse(HttpListenerRequest request, ref SQLDatabase db)
		{
			if (request.HttpMethod == "POST" && request.Headers["Content-Type"] == "application/x-www-form-urlencoded")
			{
				using (var sr = new StreamReader(request.InputStream, true))
				{
					var b = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n";
					var param = string.Empty;
					var postdata = Functions.EncodeTo(sr.ReadToEnd(), request.ContentEncoding, request.ContentEncoding).Split('&');
					var t = string.Empty;
					var l = string.Empty;

					for (var i = uint.MinValue; i < postdata.Length; i++)
					{
						if (!postdata[i].Contains("="))
							continue;

						var entry = postdata[i].Split('=');
						if (entry[0].StartsWith("--"))
						{
							param = entry[0];

							if (param == "--cname")
								t = "HostName";

							if (param == "--model")
								t = "Model";

							if (param == "--macaddr")
								t = "MacAddress";

							if (param == "--sernum")
								t = "SerialNumber";

							if (param == "--user")
								t = "Username";

							if (param == "--make")
								t = "Make";

							if (param == "--wgroup")
								t = "WorkGroup";

							if (param == "--owner")
								t = "Owner";

							if (param == "--wsus")
								t = "WUServer";

							if (param == "--kms")
								t = "SLServer";

							continue;
						}

						l = Functions.GetComputerEntry("Computer", entry[1], t, param, ref db);

						if (!string.IsNullOrEmpty(l))
							Console.WriteLine("Got request from '{0}' for '{1}' -> {2}", request.RemoteEndPoint, param, l);
					}

					b += "<string xmlns=\"{0}://{1}/mdt/\">{2}</string>".F(request.IsSecureConnection ? "https" : "http", request.Headers["Host"], l);

					return b;
				}
			}

			return string.Empty;
		}
	}
}
