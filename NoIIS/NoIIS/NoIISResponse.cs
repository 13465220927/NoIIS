using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Net;

namespace NoIIS
{
	public class NoIISResponse : HttpResponseBase, IDisposable
	{
		private HttpListenerResponse response = null;
		private StreamWriter writer = null;
		
		public NoIISResponse(HttpListenerContext context) : base()
		{
			/*
			 * Necessary:
			 *    - StatusCode
			 *    - Output
			 *    - OutputStream
			 *    - ContentType
			 */
			
			this.response = context.Response;
			this.writer = new StreamWriter(this.response.OutputStream);
		}

		public void Dispose()
		{
			this.Flush();
		}
	
		public override void Flush()
		{
			try
			{
				this.writer.Dispose();
			}
			catch
			{
			}
			
			try
			{
				this.response.OutputStream.Dispose();
			}
			catch
			{
			}
		}
		
		public override void WriteFile(string filename)
		{
			if(filename == null)
			{
				return;
			}
			
			if(!File.Exists(filename))
			{
				return;
			}
			
			using(var inputStream = File.OpenRead(filename))
			{
				inputStream.CopyTo(this.response.OutputStream);
			}
		}
		
		public override NameValueCollection Headers
		{
			get
			{
				var result = new NameValueCollection();
				foreach(var key in this.response.Headers.AllKeys)
				{
					result.Add(key, this.response.Headers[key]);
				}
				
				return result;
			}
		}
		
		public override string RedirectLocation
		{
			get
			{
				return this.response.RedirectLocation;
			}
			
			set
			{
				this.response.RedirectLocation = value;
			}
		}
		
		public override void Redirect(string url)
		{
			this.response.Redirect(url);
		}
		
		public new HttpCookieCollection Cookies
		{
			get
			{
				var result = new HttpCookieCollection();
				foreach(Cookie c in this.response.Cookies)
				{
					result.Add(new HttpCookie(c.Name, c.Value));
				}
				
				return result;
			}
			
			set
			{
				var result = new CookieCollection();
				foreach(HttpCookie c in value)
				{
					result.Add(new Cookie(c.Name, c.Value, c.Path, c.Domain));
				}
				
				this.response.Cookies = result;
			}
		}
		
		public override Encoding ContentEncoding
		{
			get
			{
				return this.response.ContentEncoding;
			}
			
			set
			{
				this.response.ContentEncoding = value;
			}
		}
		
		public override void Close()
		{
			this.response.Close();
		}
		
		public override void SetCookie(HttpCookie cookie)
		{
			this.response.SetCookie(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
		}
		
		public override void AppendCookie(HttpCookie cookie)
		{
			this.response.AppendCookie(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
		}
		
		public override void AppendHeader(string name, string value)
		{
			this.response.AppendHeader(name, value);
		}
		
		public override void AddHeader(string name, string value)
		{
			this.response.AddHeader(name, value);
		}
		
		public override int StatusCode
		{
			get
			{
				return this.response.StatusCode;
			}
			
			set
			{
				this.response.StatusCode = value;
			}
		}
		
		public override TextWriter Output
		{
			get
			{
				return this.writer;
			}
			
			set
			{
				this.writer = (StreamWriter)value;
			}
		}
		
		public override Stream OutputStream
		{
			get
			{
				return this.response.OutputStream;
			}
		}
		
		public override string ContentType
		{
			get
			{
				return this.response.ContentType;
			}
			
			set
			{
				this.response.ContentType = value;
			}
		}
	}
}
