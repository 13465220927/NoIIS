using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;
using System.Net;

namespace NoIIS
{
	/// <summary>
	/// This is a class for a web server request which is compatible to the IIS. Normally, you dont need
	/// to create an object by your own. NoIIS will handle this for you. This class is not thread-safe.
	/// Please use it only at one thread at a time!
	/// </summary>
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
		private string tmpRequestTMPFolder = Guid.NewGuid().ToString(); // The tmp. folder name for this request
		private string tmpRequestTMPFolderPATH = string.Empty; // The tmp. folder's path for this request
		private bool tmpRequestFileCreated = false; // Was the tmp. file created?
		private string tmpFolder = string.Empty; // The global tmp. folder for the server
		private string tmpRequestTMPFilePATH = string.Empty; // The tmp. file for this request's body
		private HttpListenerContext context = null;
		private List<Stream> probablyOpenStreams = new List<Stream>();
		
		/// <summary>
		/// The constructor for this class. Normally, you dont have to use it by your own.
		/// NoIIS will handle it for you.
		/// </summary>
		/// <param name="context">The HTTP listener context.</param>
		/// <param name="tempFolder">A temporary folder for this request. It will be used e.g. for uploaded files.</param>
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
			this.inputStream = this.InputStream; // Creates the unique tmp. file for this request and propagates the tmp* variables
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
			this.files = context.Request.GetFiles(this.InputStream, this.tmpRequestTMPFolderPATH, this);
			this.form = context.Request.GetForm(this.InputStream);
			this.parameters = this.QueryString.Merge(this.Form);
		}

		/// <summary>
		/// This class is disposable.
		/// </summary>
		public void Dispose()
		{
			// Close the init. stream:
			try
			{
				this.inputStream.Dispose();
			}
			catch
			{
			}
			
			// Try to close all probably open streams:
			if(this.probablyOpenStreams != null)
			{
				foreach(var probablyOpenStream in this.probablyOpenStreams)
				{
					try
					{
						probablyOpenStream.Dispose();
					}
					catch
					{
					}
				}
				
				this.probablyOpenStreams.Clear();
			}
			
			// Try to delete all tmp. files:
			try
			{
				Directory.Delete(this.tmpRequestTMPFolderPATH, true);
			}
			catch
			{
			}
		}
	
		/// <summary>
		/// Gets all the files, which are uploaded by the client.
		/// </summary>
		public override HttpFileCollectionBase Files
		{
			get
			{
				return this.files;
			}
		}
		
		/// <summary>
		/// Gets all languages of the client.
		/// </summary>
		public override string[] UserLanguages
		{
			get
			{
				return this.userLanguages;
			}
		}
		
		/// <summary>
		/// Gets the hostname of the client.
		/// </summary>
		public override string UserHostName
		{
			get
			{
				return this.userHostName;
			}
		}
		
		/// <summary>
		/// Gets the address of the client.
		/// </summary>
		public override string UserHostAddress
		{
			get
			{
				return this.userHostAddress;
			}
		}
		
		/// <summary>
		/// Gets the user agent of the client.
		/// </summary>
		public override string UserAgent
		{
			get
			{
				return this.userAgent;
			}
		}
		
		/// <summary>
		/// Gets the called URL.
		/// </summary>
		public override Uri Url
		{
			get
			{
				return this.url;
			}
		}
		
		/// <summary>
		/// Gets the referrer.
		/// </summary>
		public override Uri UrlReferrer
		{
			get
			{
				return this.urlReferrer;
			}
		}
		
		/// <summary>
		/// Gets the total amount of bytes of this request.
		/// </summary>
		public override int TotalBytes
		{
			get
			{
				return this.contentLength;
			}
		}
		
		/// <summary>
		/// Gets all parameters which are passed by using the GET method i.e. passed as query string.
		/// </summary>
		public override NameValueCollection QueryString
		{
			get
			{
				return this.queryString;
			}
		}
		
		/// <summary>
		/// Gets all header fields of the request.
		/// </summary>
		public override NameValueCollection Headers
		{
			get
			{
				return this.headers;
			}
		}
		
		/// <summary>
		/// Gets the path of this request.
		/// </summary>
		public override string Path
		{
			get
			{
				return this.path;
			}
		}
		
		/// <summary>
		/// Gets the raw path of this request.
		/// </summary>
		public override string RawUrl
		{
			get
			{
				return this.rawUrl;
			}
		}
		
		/// <summary>
		/// Gets the path info for this request.
		/// </summary>
		public override string PathInfo
		{
			get
			{
				return this.pathInfo;
			}
		}
		
		/// <summary>
		/// Returns true, if the request was using SSL/TLS.
		/// </summary>
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
		
		/// <summary>
		/// Gets the HTTP method e.g. GET, POST, etc.
		/// </summary>
		public override string HttpMethod
		{
			get
			{
				return this.httpMethod;
			}
		}
		
		/// <summary>
		/// Gets the HTTP method e.g. GET, POST, etc.
		/// </summary>
		public override string RequestType
		{
			get
			{
				return this.httpMethod;
			}
		}

        /// <summary>
        /// Read count number of bytes of the request and returns the byte array.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The byte array.</returns>
        public override byte[] BinaryRead(int count)
        {
            if(count < 1) {
                return new byte[0];
            }

            var data = new byte[count];
            if(count > this.ContentLength)
            {
                count = this.ContentLength;
            }

            try
            {
                using(var inputStream = this.InputStream)
                {
                    inputStream.Read(data, 0, count);
                }

                return data;
            }
            catch
            {
                return data;
            }
        }

        /// <summary>
        /// Gets the input stream from the client, to read the body of this request.
        /// </summary>
        public override Stream InputStream
		{
			get
			{
				// Is the tmp. file already created?
				if(!this.tmpRequestFileCreated)
				{
					// Generate the tmp. folder name for this request:
					this.tmpRequestTMPFolderPATH = this.tmpFolder + this.tmpRequestTMPFolder + global::System.IO.Path.DirectorySeparatorChar;
					
					// Create the folder:
					try
					{
						Directory.CreateDirectory(this.tmpRequestTMPFolderPATH);
					}
					catch(Exception e)
					{
						Console.WriteLine("Exception while creating the temporary folder '{1}' for the request: {0}", e.Message, this.tmpRequestTMPFolderPATH);
					   	Console.WriteLine(e.StackTrace);
					   	this.tmpRequestFileCreated = false;
					   	return new MemoryStream(new byte[0], false);
					}
					
					// Generate the tmp. file name:
					this.tmpRequestTMPFilePATH = this.tmpRequestTMPFolderPATH + Guid.NewGuid().ToString();
					using(var inputStream = this.context.Request.InputStream)
					{
						using(var fileStream = File.OpenWrite(this.tmpRequestTMPFilePATH))
						{
							inputStream.CopyTo(fileStream);
						}
					}
					
					this.tmpRequestFileCreated = true;
				}
				
				// Return a fresh stream to the body:
				var stream = File.OpenRead(this.tmpRequestTMPFilePATH);
				this.AddProbablyOpenStream(stream);
				return stream;
			}
		}
		
		/// <summary>
		/// Gets the input stream from the client, to read the body of this request.
		/// </summary>
		public override Stream GetBufferedInputStream()
		{
			return this.InputStream;
		}
		
		/// <summary>
		/// Gets the input stream from the client, to read the body of this request.
		/// </summary>
		public override Stream GetBufferlessInputStream()
		{
			return this.InputStream;
		}
		
		/// <summary>
		/// Gets the input stream from the client, to read the body of this request.
		/// </summary>
		public override Stream GetBufferlessInputStream(bool disableMaxRequestLength)
		{
			return this.InputStream;
		}
		
		/// <summary>
		/// Gets all parameters of this request, does not matter from where (query string, form values).
		/// </summary>
		public override NameValueCollection Params
		{
			get
			{
				return this.parameters;
			}
		}
		
		/// <summary>
		/// Gets all parameters of this request from the form values.
		/// </summary>
		public override NameValueCollection Form
		{
			get
			{
				return this.form;
			}
		}
		
		/// <summary>
		/// Gets the file path of this request.
		/// </summary>
		public override string FilePath
		{
			get
			{
				return this.filePath;
			}
		}
		
		/// <summary>
		/// Gets the content type of this request.
		/// </summary>
		public override string ContentType
		{
			get
			{
				return this.contentType;
			}
		}
		
		/// <summary>
		/// Gets the types which the client accepts.
		/// </summary>
		public override string[] AcceptTypes
		{
			get
			{
				return this.acceptTypes;
			}
		}
		
		/// <summary>
		/// Gets the content encoding for this request.
		/// </summary>
		public override Encoding ContentEncoding
		{
			get
			{
				return this.contentEncoding;
			}
		}
		
		/// <summary>
		/// Gets the content length of this request.
		/// </summary>
		public override int ContentLength
		{
			get
			{
				return this.contentLength;
			}
		}
		
		/// <summary>
		/// Gets all cookies from this request.
		/// </summary>
		public override HttpCookieCollection Cookies
		{
			get
			{
				return this.cookies;
			}
		}
		
		/// <summary>
		/// Add another stream which we should close at the dispose time.
		/// </summary>
		/// <param name="probablyOpenStream">The stream.</param>
		internal void AddProbablyOpenStream(Stream probablyOpenStream)
		{
			if(probablyOpenStream != null)
			{
				this.probablyOpenStreams.Add(probablyOpenStream);
			}
		}
	}
}