using System;
using System.Net;
using System.Collections.Specialized;
using MDTWebService.Klassen;

namespace MDTWebService
{
	public static class MDTWebInterface
	{
		static string HTML_footer()
		{
			var pagecontent = string.Empty;
			pagecontent += "\t</body>\n";
			pagecontent += "</html>\n";

			return pagecontent;
		}

		static string HTML_header(string charset, bool errorpage = false)
		{
			var pagecontent = string.Empty;

			pagecontent += "<!DOCTYPE html>\n";
			pagecontent += "<html>\n";
			pagecontent += "\t<head>\n";
			pagecontent += "\t\t<title></title>\n";
			pagecontent += "\t\t<meta charset=\"{0}\" />\n".F(charset);
			pagecontent += "\t\t<meta http-equiv=\"expires\" content=\"0\" />\n";
			pagecontent += "\t\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, user-scalable=no\">\n";

			var xmldoc = Files.ReadXML("http/DataSets/index.xml");
			if (xmldoc != null && xmldoc.HasChildNodes)
			{
				var stylessheets = xmldoc.DocumentElement.GetElementsByTagName("stylesheet");
				for (var i = 0; i < stylessheets.Count; i++)
				{
					var attributes = stylessheets[i].Attributes;
					pagecontent += "\t\t<link href =\"/Designs/[[DESIGN]]/{0}.css\" rel=\"stylesheet\" type=\"{1}\" />\n"
					.F(attributes["src"].InnerText, attributes["type"].InnerText);
				}

				var scripts = xmldoc.DocumentElement.GetElementsByTagName("script");
				for (var i = 0; i < scripts.Count; i++)
				{
					var attributes = scripts[i].Attributes;
					var path = "/scripts/{0}.js".F(attributes["src"].InnerText);
					pagecontent += "\t\t<script type=\"{0}\" src=\"{1}\"></script>\n".F(attributes["type"].InnerText, path);
				}
			}

			pagecontent += "\t</head>\n";
			pagecontent += "\t<body>\n";

			return pagecontent;
		}

		public static byte[] HandleWebInterFaceRequest(ref HttpListenerRequest request, ref SQLDatabase db)
		{
			var length = ulong.MinValue;
			var url = ParseRequest(request.Url.LocalPath != null ? request.Url.LocalPath.ToLowerInvariant() : 
				"/", request.QueryString, request.HttpMethod, out length);

			if (string.IsNullOrEmpty(url))
				return new byte[0];

			if (!Filesystem.Exist(url) && request.HttpMethod == "GET")
				return new byte[0];

			var data = new byte[length];
			var bytesRead = 0;

			Files.Read(url, ref data, out bytesRead);

			if (url.EndsWith(".htm") || url.EndsWith(".html") || url.EndsWith(".js") || url.EndsWith(".css"))
			{
				var pagecontent = string.Empty;
				if (url.EndsWith(".htm") || url.EndsWith(".html"))
				{
					pagecontent += HTML_header("utf-8");

					if (url.EndsWith("index.html"))
					{
						pagecontent += "\t\t<div id=\"page\">\n";

						pagecontent += "\t\t\t<nav>\n";
						pagecontent += "\t\t\t</nav>\n";

						pagecontent += "\t\t<header>\n";
						pagecontent += "\t\t<h2></h2>\n";
						pagecontent += "\t\t</header>\n";
					}

					pagecontent += "\t<script type=\"text/javascript\">\n";
					pagecontent += "\t\tdocument.getElementById('anchor_summary').click();\n";
					pagecontent += "\t</script>\n";
					pagecontent += "\t\t\t<main>\n";
				}

				pagecontent += Exts.EncodeTo(data, request.ContentEncoding);

				if (url.EndsWith(".htm") || url.EndsWith(".html"))
				{
					pagecontent = pagecontent.Replace("[[DESIGN]]", "Default");
					pagecontent += "<div class=\"table\">\n";

					/*
					var q = db.SQLQuery("SELECT * FROM Computer LIMIT 128");
					for (var i = uint.MinValue; i < q.Count;i++)
						pagecontent += "<div class=\"tr\">\n<div class=\"td\">{0}</div>\n<div class=\"td\">{1}</div>\n<div class=\"td\">{2}</div>\n<div class=\"td\">{3}</div>\n</div>\n"
							.F(q[i]["Make"], q[i]["Model"], q[i]["SerialNumber"], q[i]["UUID"]);
					*/
					pagecontent += "</div>";
					pagecontent += "\t\t\t</main>\n";
					pagecontent += "\t\t</div>\n";
					pagecontent += HTML_footer();
				}

				data = request.ContentEncoding.GetBytes(pagecontent);
			}

			return data;
		}

		static string ParseRequest(string url, NameValueCollection arguments, string method, out ulong length)
		{
			try
			{
				var retval = url;

				if (method == "POST" && url == "/settings.html")
					retval = "/";

				if (retval == "/approve.html")
					retval = "/requests.html";

				if (retval == "/")
					retval = "/index.html";

				if (!retval.EndsWith(".htm") && !retval.EndsWith(".html") && !retval.EndsWith(".js") &&
					!retval.EndsWith(".css") && !retval.EndsWith(".png") && !retval.EndsWith(".gif") && !retval.EndsWith(".ico"))
					throw new Exception("Unsupported Content type!");

				var size = Filesystem.Size("http{0}".F(retval));
				length = (ulong)size;

				if (size > 10485760) // 10 MB
					return string.Empty;

				return "http{0}".F(retval);
			}
			catch
			{
				length = ulong.MinValue;
				return string.Empty;
			}
		}
	}
}
