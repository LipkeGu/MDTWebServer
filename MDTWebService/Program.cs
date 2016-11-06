using MDTWebService.Klassen;
using System;

namespace MDTWebService
{
	class Program
	{
		static SQLDatabase db = new SQLDatabase();
		static HTTPSocket ws = new HTTPSocket(81, "mdt/");
		static HTTPSocket web = new HTTPSocket(82, "");

		static void Main(string[] args)
		{
			ws.HTTPDataReceived += Ws_HTTPDataReceived;
			ws.HTTPDataSend += Ws_HTTPDataSend;

			web.HTTPDataReceived += Web_HTTPDataReceived;
			web.HTTPDataSend += Web_HTTPDataSend;

			var x = string.Empty;
			while (x != "-exit")
				x = Console.ReadLine();

			web.Dispose();
			ws.Dispose();
			db.Close();
		}

		private static void Web_HTTPDataSend(object sender, HTTPDataSendEventArgs e) {}
		private static void Ws_HTTPDataSend(object sender, HTTPDataSendEventArgs e) {}

		private static void Web_HTTPDataReceived(object sender, HTTPDataReceivedEventArgs e)
		{
			web.Send(MDTWebInterface.HandleWebInterFaceRequest(ref e.Request, ref db), 200, "OK");
		}

		private static void Ws_HTTPDataReceived(object sender, HTTPDataReceivedEventArgs e)
		{
			ws.Send(e.Request.ContentEncoding.GetBytes(MDTWebProvider.SendResponse(e.Request, ref db)), 200, "OK");
		}
	}
}
