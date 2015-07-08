using System;
using System.IO;
using System.Web;

namespace NoIIS
{
	/// <summary>
	/// This class is an implementation of the HttpPostedFileBase class from System.Web.
	/// </summary>
	public class NoIISPostedFile : HttpPostedFileBase
	{
		private int contentLength = 0;
		private string contentType = string.Empty;
		private string fileName = string.Empty;
		private string formName = string.Empty;
		private Stream inputStream = null;
		private string tmpFilenamePATH = string.Empty;
		
		public NoIISPostedFile() : base()
		{
		}
		
		public NoIISPostedFile(int contentLength, string contentType, string filename, string formName, Stream data) : base()
		{
			this.contentLength = contentLength;
			this.contentType = contentType;
			this.fileName = filename;
			this.inputStream = data;
		}
		
		public override int ContentLength
		{
			get
			{
				return this.contentLength;
			}
		}
		
		internal void setContentLength(int lenBytes)
		{
			this.contentLength = lenBytes;
		}
		
		public override string ContentType
		{
			get
			{
				return this.contentType;
			}
		}
		
		internal void setContentType(string contentsType)
		{
			this.contentType = contentsType;
		}
		
		public override bool Equals(object obj)
		{
			var other = obj as NoIISPostedFile;
			if(other == null)
			{
				return false;
			}
			
			if(this.contentLength != other.contentLength)
			{
				return false;
			}
			
			if(this.contentType != other.contentType)
			{
				return false;
			}
			
			if(this.fileName != other.fileName)
			{
				return false;
			}
			
			if(this.formName != other.formName)
			{
				return false;
			}
			
			if(this.inputStream == null || other.inputStream == null || this.inputStream != other.inputStream)
			{
				return false;
			}
			
			return true;
		}

		public override int GetHashCode()
		{
			return this.inputStream.GetHashCode();
		}
		
		public override string FileName
		{
			get
			{
				return this.fileName;
			}
		}
		
		internal void setFileName(string name)
		{
			this.fileName = name;
		}
		
		public override Stream InputStream
		{
			get
			{
				return this.inputStream;
			}
		}
		
		internal void setInputStream(Stream stream)
		{
			this.inputStream = stream;
		}
		
		/// <summary>
		/// Stores this file to the local file system.
		/// </summary>
		/// <param name="filename">The local destination. If the file existis, overwrites it.</param>
		public override void SaveAs(string filename)
		{
			if(File.Exists(filename))
			{
				File.Delete(filename);
			}
			
			if(this.inputStream == null)
			{
				return;
			}
			
			using(var file = File.OpenWrite(filename))
			{
				this.inputStream.CopyTo(file);
			}
		}
		
		public bool IsEmpty()
		{
			return this.contentLength == 0 && this.inputStream == null && this.contentType == string.Empty && this.fileName == string.Empty;
		}

		internal string FormName
		{
			get
			{
				return this.formName;
			}
			
			set
			{
				this.formName = value;
			}
		}
		
		internal string TMPFilenamePATH
		{
			get
			{
				return tmpFilenamePATH;
			}
			
			set
			{
				tmpFilenamePATH = value;
			}
		}
	}
}
