using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Net;

namespace NoIIS
{
	public class NoIISRequest : HttpRequestBase, IDisposable
	{
		private string[] acceptTypes = new string[0];
		private Encoding contentEncoding = ASCIIEncoding.UTF8;
		private int contentLength = 0;
		private HttpCookieCollection cookies = new HttpCookieCollection();
		private string contentType = string.Empty;
		private string filePath = string.Empty;
		private NameValueCollection form = null;
		private NameValueCollection parameters = null;
		private Stream inputStream = null;
		private NameValueCollection headers = null;
		private string httpMethod = "GET";
		private bool isAuthenticated = false;
		private bool isLocal = false;
		private bool isSecureConnection = false;
		private string pathInfo = string.Empty;
		private string rawUrl = string.Empty;
		private string path = string.Empty;
		private NameValueCollection queryString = null;
		private Uri url = null;
		private Uri urlReferrer = null;
		private string userAgent = string.Empty;
		private string userHostAddress = string.Empty;
		private string userHostName = string.Empty;
		private string[] userLanguages = new string[0];
		private HttpFileCollectionBase files = null;
		private string tmpFilename = Guid.NewGuid().ToString();
		private string tmpFolder = string.Empty;
		private HttpListenerContext context = null;
		
		public NoIISRequest(HttpListenerContext context, string tempFolder)
		{
			/*
			 * TODO Missing properties:
			 * - AnonymousID
			 * - ApplicationPath
			 * - AppRelativeCurrentExecutionFilePath
			 * - Browser
			 * - ClientCertificate
			 * - CurrentExecutionFilePath
			 * - CurrentExecutionFilePathExtension
			 * - Filter
			 * - HttpChannelBinding
			 * - LogonUserIdentity
			 * - PhysicalApplicationPath
			 * - PhysicalPath
			 * - ReadEntityBodyMode
			 * - RequestContext
			 * - ServerVariables
			 * - TimedOutToken
			 * - Unvalidated
			 * 
			 * TODO Missing methods:
			 * - BinaryRead()
			 * - InsertEntityBody()
			 * - MapImageCoordinates()
			 * - MapPath()
			 * - MapRawImageCoordinates()
			 * - SaveAs()
			 * - ToString()
			 * - ValidateInput()
			 */
			
			this.context = context;
			this.tmpFolder = tempFolder;
			this.acceptTypes = context.Request.AcceptTypes;
			this.contentEncoding = context.Request.ContentEncoding;
			this.contentLength = (int)context.Request.ContentLength64;
			
			foreach(Cookie c in context.Request.Cookies)
			{
				this.cookies.Add(new HttpCookie(c.Name, c.Value));
			}
			
			this.contentType = context.Request.ContentType;
			this.rawUrl = context.Request.Url.PathAndQuery;
			this.filePath = context.Request.Url.FilePath();
			this.pathInfo = context.Request.Url.PathInfo();
			this.path = context.Request.Url.LocalPath;
			this.queryString = context.Request.RawUrl.GetQueryString();
			this.inputStream = null;
			this.headers = context.Request.Headers;
			this.httpMethod = context.Request.HttpMethod;
			this.isAuthenticated = context.Request.IsAuthenticated;
			this.isLocal = context.Request.IsLocal;
			this.isSecureConnection = context.Request.IsSecureConnection;
			this.url = context.Request.Url;
			this.urlReferrer = context.Request.UrlReferrer;
			this.userAgent = context.Request.UserAgent;
			this.userHostAddress = context.Request.UserHostAddress;
			this.userHostName = context.Request.UserHostName;
			this.userLanguages = context.Request.UserLanguages;
			this.files = context.Request.GetFiles(tempFolder, this.tmpFilename);
			this.form = context.Request.GetForm(tempFolder, this.tmpFilename);
			this.parameters = this.QueryString.Merge(this.Form);
		}

		public void Dispose()
		{
			try
			{
				this.inputStream.Dispose();
			}
			catch
			{
			}
			
			try
			{
				File.Delete(this.tmpFolder + this.tmpFilename);
			}
			catch
			{
			}
		}
	
		public override HttpFileCollectionBase Files
		{
			get
			{
				return this.files;
			}
		}
		
		public override string[] UserLanguages
		{
			get
			{
				return this.userLanguages;
			}
		}
		
		public override string UserHostName
		{
			get
			{
				return this.userHostName;
			}
		}
		
		public override string UserHostAddress
		{
			get
			{
				return this.userHostAddress;
			}
		}
		
		public override string UserAgent
		{
			get
			{
				return this.userAgent;
			}
		}
		
		public override Uri Url
		{
			get
			{
				return this.url;
			}
		}
		
		public override Uri UrlReferrer
		{
			get
			{
				return this.urlReferrer;
			}
		}
		
		public override int TotalBytes
		{
			get
			{
				return this.contentLength;
			}
		}
		
		public override NameValueCollection QueryString
		{
			get
			{
				return this.queryString;
			}
		}
		
		public override NameValueCollection Headers
		{
			get
			{
				return this.headers;
			}
		}
		
		public override string Path
		{
			get
			{
				return this.path;
			}
		}
		
		public override string RawUrl
		{
			get
			{
				return this.rawUrl;
			}
		}
		
		public override string PathInfo
		{
			get
			{
				return this.pathInfo;
			}
		}
		
		public override bool IsSecureConnection
		{
			get
			{
				return this.isSecureConnection;
			}
		}
		
		public override bool IsLocal
		{
			get
			{
				return this.isLocal;
			}
		}
		
		public override bool IsAuthenticated
		{
			get
			{
				return this.isAuthenticated;
			}
		}
		
		public override string HttpMethod
		{
			get
			{
				return this.httpMethod;
			}
		}
		
		public override string RequestType
		{
			get
			{
				return this.httpMethod;
			}
		}
		
		public override Stream InputStream
		{
			get
			{
				if(this.inputStream == null)
				{
					var tmpFile = this.tmpFolder + this.tmpFilename;
					using(var inputStream = this.context.Request.InputStream)
					{
						using(var fileStream = File.OpenWrite(tmpFile))
						{
							inputStream.CopyTo(fileStream);
						}
					}
					
					this.inputStream = File.OpenRead(tmpFile);
				}
				
				return this.inputStream;
			}
		}
		
		public override Stream GetBufferedInputStream()
		{
			return this.inputStream;
		}
		
		public override Stream GetBufferlessInputStream()
		{
			return this.inputStream;
		}
		
		public override Stream GetBufferlessInputStream(bool disableMaxRequestLength)
		{
			return this.inputStream;
		}
		
		public override NameValueCollection Params
		{
			get
			{
				return this.parameters;
			}
		}
		
		public override NameValueCollection Form
		{
			get
			{
				return this.form;
			}
		}
		
		public override string FilePath
		{
			get
			{
				return this.filePath;
			}
		}
		
		public override string ContentType
		{
			get
			{
				return this.contentType;
			}
		}
		
		public override string[] AcceptTypes
		{
			get
			{
				return this.acceptTypes;
			}
		}
		
		public override Encoding ContentEncoding
		{
			get
			{
				return this.contentEncoding;
			}
		}
		
		public override int ContentLength
		{
			get
			{
				return this.contentLength;
			}
		}
		
		public override HttpCookieCollection Cookies
		{
			get
			{
				return this.cookies;
			}
		}
	}
}