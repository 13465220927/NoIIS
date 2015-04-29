using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;

namespace NoIIS
{
	public static class Extensions
	{
		public static NameValueCollection GetForm(this HttpListenerRequest request, string tempFolder, string tempFilename)
		{
			if(request == null || tempFolder == null)
			{
				return new NameValueCollection();
			}
			
			if(!request.HasEntityBody)
			{
				return new NameValueCollection();
			}
			
			// Case #1: Just a form
			//		Content Type = application/x-www-form-urlencoded
			//		Body = MAX_FILE_SIZE=100000&t%C3%A4stValue=das+ist+ein+test
			if(request.ContentType == "application/x-www-form-urlencoded")
			{
				using(var inputStream = request.InputStream)
				{
					using(var reader = new StreamReader(inputStream))
					{
						return reader.ReadToEnd().GetQueryString();
					}
				}
			}
			
			// Case #2: Just a file
			// 		Content type = multipart/form-data; boundary=----WebKitFormBoundaryeghC9TqseFEkpCXg
			// 		Body = 
			//      ------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFileö"; filename="Short Time Notes.txt"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg--
			
			//
			// ==> Does not matter
			//
			
			// Case #3: Multiple files
			// 		Content type = multipart/form-data; boundary=----WebKitFormBoundaryeghC9TqseFEkpCXg
			// 		Body = 
			//      ------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFileö"; filename="Short Time Notes.txt"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFile2"; filename="fsdfsdf.txt"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg--
			
			//
			// ==> Does not matter
			//
			
			// Case #4: A file and form data
			// 		Content type = multipart/form-data; boundary=----WebKitFormBoundaryeghC9TqseFEkpCXg
			// 		Body = 
			//      ------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFileö"; filename="Short Time Notes.txt"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//		Content-Disposition: form-data; name="einsÖÄÜ"
			//		
			//		2354öäpü
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//		Content-Disposition: form-data; name="zwei"
			//		
			//		ein@test.de
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg--
			
			//
			// ==> Does not matter, use case #5.
			//

			// Case #5: Multiple files and form data
			// 		Content type = multipart/form-data; boundary=----WebKitFormBoundaryeghC9TqseFEkpCXg
			// 		Body = 
			//      ------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFileö"; filename="Short Time Notes.txt"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//		Content-Disposition: form-data; name="einsÖÄÜ"
			//		
			//		2354öäpü
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//		Content-Disposition: form-data; name="zwei"
			//		
			//		ein@test.de
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFile3"; filename="Ssdfsdfd.fdf"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//		Content-Disposition: form-data; name="rterz"
			//		
			//		ein@test.deasdasdsa
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg--
			
			// Expect a huge amount of data (GBs or more)!
			var tempFile = tempFolder + tempFilename;
			if(!File.Exists(tempFile))
			{
				using(var inputStream = request.InputStream)
				{
					using(var fileStream = File.OpenWrite(tempFile))
					{
						inputStream.CopyTo(fileStream);
					}
				}
			}
			
			using(var fileStream = File.OpenRead(tempFile))
			{
				using(var reader = new StreamReader(fileStream))
				{
					var result = new NameValueCollection();
					while(!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						if(line != null && line.Length > 38 && line[0] == 'C' && line.StartsWith(@"Content-Disposition: form-data; name=""") && !line.Contains(@"; filename="""))
						{
							var name = line.Substring(38, line.Length - 38 - 1);
							var dummy = reader.ReadLine();
							var value = reader.ReadLine();
							if(dummy == null || value == null)
							{
								continue;
							}
							
							result.Add(name, value);
						}
					}
					
					return result;
				}
			}
		}
		
		public static HttpFileCollectionBase GetFiles(this HttpListenerRequest request, string tempFolder, string tempFilename)
		{			
			if(request == null || tempFolder == null)
			{
				return null;
			}
			
			if(!request.HasEntityBody)
			{
				return null;
			}
			
			// Case #1: Just a form
			//		Content Type = application/x-www-form-urlencoded
			//		Body = MAX_FILE_SIZE=100000&t%C3%A4stValue=das+ist+ein+test
			
			//
			// ==> Does not matter
			//
			
			// Case #2: Just a file
			// 		Content type = multipart/form-data; boundary=----WebKitFormBoundaryeghC9TqseFEkpCXg
			// 		Body = 
			//      ------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFileö"; filename="Short Time Notes.txt"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg--
			
			//
			// ==> Does not matter
			//
			
			// Case #3: Multiple files
			// 		Content type = multipart/form-data; boundary=----WebKitFormBoundaryeghC9TqseFEkpCXg
			// 		Body = 
			//      ------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFileö"; filename="Short Time Notes.txt"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFile2"; filename="fsdfsdf.txt"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg--
			
			//
			// ==> Does not matter
			//
			
			// Case #4: A file and form data
			// 		Content type = multipart/form-data; boundary=----WebKitFormBoundaryeghC9TqseFEkpCXg
			// 		Body = 
			//      ------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFileö"; filename="Short Time Notes.txt"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//		Content-Disposition: form-data; name="einsÖÄÜ"
			//		
			//		2354öäpü
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//		Content-Disposition: form-data; name="zwei"
			//		
			//		ein@test.de
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg--
			
			//
			// ==> Does not matter, use case #5.
			//

			// Case #5: Multiple files and form data
			// 		Content type = multipart/form-data; boundary=----WebKitFormBoundaryeghC9TqseFEkpCXg
			// 		Body = 
			//      ------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFileö"; filename="Short Time Notes.txt"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//		Content-Disposition: form-data; name="einsÖÄÜ"
			//		
			//		2354öäpü
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//		Content-Disposition: form-data; name="zwei"
			//		
			//		ein@test.de
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//      Content-Disposition: form-data; name="meinFile3"; filename="Ssdfsdfd.fdf"
			//      Content-Type: text/plain
			//		
			//		DATA STREAM
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg
			//		Content-Disposition: form-data; name="rterz"
			//		
			//		ein@test.deasdasdsa
			//		------WebKitFormBoundaryeghC9TqseFEkpCXg--
			
			// Expect a huge amount of data (GBs or more)!
			var tempFile = tempFolder + tempFilename;
			if(!File.Exists(tempFile))
			{
				using(var inputStream = request.InputStream)
				{
					using(var fileStream = File.OpenWrite(tempFile))
					{
						inputStream.CopyTo(fileStream);
					}
				}
			}
			
			using(var fileStream = File.OpenRead(tempFile))
			{
				using(var reader = new StreamReader(fileStream))
				{
					var result = new NameValueCollection();
					while(!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						if(line != null && line.Length > 38 && line[0] == 'C' && line.StartsWith(@"Content-Disposition: form-data; name=""") && !line.Contains(@"; filename="""))
						{
							var name = line.Substring(38, line.Length - 38 - 1);
							var dummy = reader.ReadLine();
							var value = reader.ReadLine();
							if(dummy == null || value == null)
							{
								continue;
							}
							
							result.Add(name, value);
						}
					}
					
					return null;
				}
			}
		}
		
		public static NameValueCollection GetQueryString(this string data)
		{
			var result = new NameValueCollection();
			if(data == null || data == string.Empty)
			{
				return result;
			}
			
			var elements = data.Split('?');
			if(elements.Length < 2)
			{
				return result;
			}
			
			var argsRaw = elements[1];
			var args = argsRaw.Split('&');
			foreach(var a in args)
			{
				var entries = a.Split('=');
				if(entries.Length >= 2)
				{
					result.Add(HttpUtility.UrlDecode(entries[0]), HttpUtility.UrlDecode(entries[1]));
				}
			}
			
			return result;
		}
		
		public static NameValueCollection Merge(this NameValueCollection collection, NameValueCollection another)
		{
			if(collection == null && another == null)
			{
				return new NameValueCollection();
			}
			
			if(collection == null && another != null)
			{
				return new NameValueCollection(another);
			}
			
			if(another == null && collection != null)
			{
				return new NameValueCollection(collection);
			}
			
			var result = new NameValueCollection(collection);
			result.Add(another);
			
			return result;
		}
		
		public static string PathInfo(this Uri uri)
		{
			if(uri == null)
			{
				return string.Empty;
			}
			
			var filePathStart1 = uri.LocalPath.IndexOf('.');
			if(filePathStart1 == -1)
			{
				return string.Empty;
			}
			else
			{
				var filePathStart2 = uri.LocalPath.IndexOf('/', filePathStart1);
				if(filePathStart2 == -1)
				{
					return string.Empty;
				}
				else
				{
					return uri.LocalPath.Substring(filePathStart2);
				}
			}
		}
		
		public static string FilePath(this Uri uri)
		{
			if(uri == null)
			{
				return string.Empty;
			}
			
			var filePathStart1 = uri.LocalPath.IndexOf('.');
			if(filePathStart1 == -1)
			{
				return uri.LocalPath;
			}
			else
			{
				var filePathStart2 = uri.LocalPath.IndexOf('/', filePathStart1);
				if(filePathStart2 == -1)
				{
					return uri.LocalPath;
				}
				else
				{
					return uri.LocalPath.Substring(0, filePathStart2);
				}
			}
		}
	}
}
