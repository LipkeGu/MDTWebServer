using System;
using System.Net;

namespace MDTWebService
{
	public sealed class HTTPSocket : IDisposable
	{
		HttpListener socket;
		HttpListenerContext context;

		public HTTPSocket(int port, string path)
		{
			try
			{
				this.socket = new HttpListener();

				this.socket.Prefixes.Add("http://*:{0}/{1}".F(port, path));
				this.socket.Start();

				if (this.socket.IsListening)
					this.socket.BeginGetContext(new AsyncCallback(this.GetContext), null);
			}
			catch (HttpListenerException e)
			{
				Console.WriteLine("HTTP: {0}", e);
			}
		}

		public event HTTPDataReceivedEventHandler HTTPDataReceived;
		public event HTTPDataSendEventHandler HTTPDataSend;

		public void Dispose() { this.Close(); }

		private void OnHTTPDataSend(HttpListenerContext context)
		{
			var evtargs = new HTTPDataSendEventArgs();
			this.HTTPDataSend?.Invoke(this, evtargs);
		}

		private void OnHTTPDataReceived(HttpListenerContext context)
		{
			var evtargs = new HTTPDataReceivedEventArgs();
			evtargs.Request = this.context.Request;

			this.HTTPDataReceived?.Invoke(this, evtargs);
		}

		internal void Send(byte[] buffer, int statuscode, string description)
		{
			this.context.Response.StatusCode = statuscode;
			this.context.Response.StatusDescription = description;

			using (var s = this.context.Response.OutputStream)
				try
				{
					if (s.CanWrite)
					{
						s.Write(buffer, 0, buffer.Length);
						this.OnHTTPDataSend(this.context);
					}
				}
				catch {}
		}

		private void Close()
		{
			if (this.socket != null)
				this.socket.Close();
		}

		private void GetContext(IAsyncResult ar)
		{
			this.context = this.socket.EndGetContext(ar);
			this.OnHTTPDataReceived(this.context);

			this.socket.BeginGetContext(new AsyncCallback(this.GetContext), null);
		}
	}
}
