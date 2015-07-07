using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Net;

namespace NoIIS
{
	/// <summary>
	/// This is a class for a web server response which is compatible to the IIS. Normally, you dont need
	/// to create an object by your own. NoIIS will handle this for you. This class is not thread-safe.
	/// Please use it only at one thread at a time! 
	/// </summary>
	public class NoIISResponse : HttpResponseBase, IDisposable
	{
		private HttpListenerResponse response = null;
		private StreamWriter writer = null;
		
		/// <summary>
		/// The constructor for this class. Normally, you dont have to use these by your own.
		/// NoIIS will handle it for you.
		/// </summary>
		/// <param name="context">The HTTP listener context.</param>
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

		/// <summary>
		/// The instance of this class is disposable.
		/// </summary>
		public void Dispose()
		{
			this.Flush();
		}
	
		/// <summary>
		/// The flush method which overrides HttpResponseBase's method.
		/// </summary>
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
		
		/// <summary>
		/// The WriteFile method which overrides HttpResponseBase's method. Uploads a local file
		/// to the client.
		/// </summary>
		/// <param name="filename">The local filename which you want to upload.</param>
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
		
		/// <summary>
		/// Gets the Headers of this response. Overrides HttpResponseBase's method.
		/// </summary>
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
		
		/// <summary>
		/// Get or sets the redirect location.
		/// </summary>
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
		
		/// <summary>
		/// The Redirect method which overrides HttpResponseBase's method. Redirects the client
		/// to the given URL.
		/// </summary>
		/// <param name="url">The url where the client should be redirected.</param>
		public override void Redirect(string url)
		{
			this.response.Redirect(url);
		}
		
		/// <summary>
		/// Gets or sets the cookies for this response.
		/// </summary>
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
		
		/// <summary>
		/// Gets or sets the content encoding for this response.
		/// </summary>
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
		
		/// <summary>
		/// The Close method which overrides HttpResponseBase's method.
		/// </summary>
		public override void Close()
		{
			this.response.Close();
		}
		
		/// <summary>
		/// Sets a cookie for this response.
		/// </summary>
		/// <param name="cookie"></param>
		public override void SetCookie(HttpCookie cookie)
		{
			this.response.SetCookie(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
		}
		
		/// <summary>
		/// Appends a cookie to this response.
		/// </summary>
		/// <param name="cookie"></param>
		public override void AppendCookie(HttpCookie cookie)
		{
			this.response.AppendCookie(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
		}
		
		/// <summary>
		/// Appends a header field to this response.
		/// </summary>
		/// <param name="name">The field's name.</param>
		/// <param name="value">The field's value.</param>
		public override void AppendHeader(string name, string value)
		{
			this.response.AppendHeader(name, value);
		}
		
		/// <summary>
		/// Adds a header field to this response.
		/// </summary>
		/// <param name="name">The field's name.</param>
		/// <param name="value">The field's value.</param>
		public override void AddHeader(string name, string value)
		{
			this.response.AddHeader(name, value);
		}
		
		/// <summary>
		/// Gets or sets the status code for this response.
		/// </summary>
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
		
		/// <summary>
		/// Gets or sets the TextWriter for this response. This enables you to write text data to the client.
		/// </summary>
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
		
		/// <summary>
		/// Gets the stream to the client side. Enables you to write any kind of data to the client.
		/// </summary>
		public override Stream OutputStream
		{
			get
			{
				return this.response.OutputStream;
			}
		}
		
		/// <summary>
		/// Gets or sets the content type.
		/// </summary>
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
