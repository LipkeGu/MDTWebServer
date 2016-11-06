using System;
using System.Net;

namespace MDTWebService
{
	public delegate void HTTPDataReceivedEventHandler(object sender, HTTPDataReceivedEventArgs e);
	public delegate void HTTPDataSendEventHandler(object sender, HTTPDataSendEventArgs e);

	public class HTTPDataReceivedEventArgs : EventArgs
	{
		public HttpListenerRequest Request;
	}

	public class HTTPDataSendEventArgs : EventArgs
	{
		public string BytesSend;
	}
}
