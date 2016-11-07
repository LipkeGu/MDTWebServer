using System;
using System.Net;
using System.Collections.Specialized;
using MDTWebService.Klassen;
using System.Text;

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

		static string HTML_header(string charset, ref SQLDatabase db, bool errorpage = false)
		{
			var pagecontent = "<!DOCTYPE html>\n";
			pagecontent += "<html>\n";
			pagecontent += "\t<head>\n";
			pagecontent += "\t\t<title></title>\n";
			pagecontent += "\t\t<meta charset=\"{0}\" />\n".F(charset);
			pagecontent += "\t\t<meta http-equiv=\"expires\" content=\"0\" />\n";
			pagecontent += "\t\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, user-scalable=no\">\n";

			var styles = db.SQLQuery("SELECT type, src FROM web_stylesheets");
			for (var i = uint.MinValue; i < styles.Count; i++)
				pagecontent += "\t\t<link href=\"/Designs/[[DESIGN]]/{0}.css\" rel=\"stylesheet\" type=\"{1}\" />\n".F(styles[i]["src"], styles[i]["type"]);

			var scripts = db.SQLQuery("SELECT type, src FROM web_scripts");
			for (var i = uint.MinValue; i < scripts.Count; i++)
				pagecontent += "\t\t<script type=\"{0}\" src=\"/scripts/{1}.js\"></script>\n".F(scripts[i]["type"], scripts[i]["src"]);

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
					pagecontent += HTML_header("utf-8", ref db);

					if (url.EndsWith("index.html"))
					{
						pagecontent += "\t\t<div id=\"page\">\n";

						pagecontent += "\t\t\t<nav>\n";
						pagecontent += Generate_head_bar(ref db);
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

				pagecontent += Exts.EncodeTo(data, Encoding.UTF8);

				if (url.EndsWith(".htm") || url.EndsWith(".html"))
				{
					pagecontent = pagecontent.Replace("[[DESIGN]]", "Default");
					pagecontent = pagecontent.Replace("[[SERVER_INFO_BLOCK]]", Generate_device_list(ref db));

					pagecontent += "\t\t\t</main>\n";
					pagecontent += "\t\t</div>\n";
					pagecontent += HTML_footer();
				}

				data = Encoding.UTF8.GetBytes(pagecontent);
			}

			return data;
		}

		private static string Generate_device_list(ref SQLDatabase db)
		{
			var pagecontent = string.Empty;

			pagecontent += "<table id=\"nv_cbox\">\n";
			pagecontent += "<tr id=\"nv_cbox_header\">\n<th>Hersteller</th>\n<th>Model</th>\n<th>Serial Num.</th>\n<th>UUID</th>\n</tr>\n";

			var q = db.SQLQuery("SELECT * FROM Computer LIMIT 128");
			for (var i = uint.MinValue; i < q.Count;i++)
				pagecontent += "<tr id=\"nv_cbox_content\">\n<td>{0}</td>\n<td>{1}</td>\n<td>{2}</td>\n<td>{3}</td>\n</tr>\n"
					.F(q[i]["Make"], q[i]["Model"], q[i]["SerialNumber"], q[i]["UUID"]);

			pagecontent += "</table>\n";

			return pagecontent;
		}

		private static string Generate_head_bar(ref SQLDatabase db)
		{
			var output = string.Empty;
			output += "\t\t\t\t<ul>\n";

			var root = db.SQLQuery("SELECT * FROM web_navigation");
			for (var i = uint.MinValue; i < root.Count; i++)
				output += "\t\t\t\t\t<li><a href=\"/#\" onclick=\"LoadDocument('{0}', '{1}', '{2}', '{3}')\" id=\"{4}\">{2}</a></li>\n"
					.F(root[i]["url"], root[i]["target"], root[i]["value"], root[i]["needs"], root[i]["ident"]);
	
			output += "\t\t\t\t</ul>\n";

			return output;
		}

		static string ParseRequest(string url, NameValueCollection arguments, string method, out ulong length)
		{
			try
			{
				var retval = url.ToLowerInvariant();

				if (method == "POST" && url == "/settings.html")
					retval = "/";

				if (retval == "/approve.html")
					retval = "/requests.html";

				if (retval == "/")
					retval = "/index.html";

				if (!retval.EndsWith(".htm") && !retval.EndsWith(".html") && !retval.EndsWith(".js") &&
					!retval.EndsWith(".css") && !retval.EndsWith(".png") && !retval.EndsWith(".gif"))
					throw new Exception("Unsupported Content type!");

				var size = Filesystem.Size("http{0}".F(retval));
				length = (ulong)size;

				if (size > 10485760) // 10 MB
					return string.Empty;

				return "http{0}".F(retval);
			}
			catch (Exception)
			{
				length = ulong.MinValue;
				return string.Empty;
			}
		}
	}
}
