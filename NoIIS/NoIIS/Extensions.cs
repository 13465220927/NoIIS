using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web;
using HttpMultipartParser;

namespace NoIIS
{
	/// <summary>
	/// This class provides some extension methods for the NoIIS web server. Every method is thread-safe.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Processes the request to get the form values.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="bodyStream">The stream for the request's body.</param>
		/// <returns>The collection with all values from the form. The collection is possibly empty.</returns>
		public static NameValueCollection GetForm(this HttpListenerRequest request, Stream bodyStream)
		{
			if(request == null || bodyStream == null)
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
			if(request.ContentType.Contains("application/x-www-form-urlencoded"))
			{
				using(bodyStream)
				{
					using(var reader = new StreamReader(bodyStream))
					{
						return reader.ReadToEnd().GetQueryString(true);
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
			
			// Check the requirements:
			if(!request.ContentType.Contains("multipart/form-data; boundary="))
			{
				return new NameValueCollection();
			}
			
			// Store the result here:
			var result = new NameValueCollection();
			
			using(bodyStream)
			{
				// Dummy to consume the name of each parameter:
				var dummy = string.Empty;
				
				// Create the parser for the body:
				var smftp = new StreamingMultipartFormDataParser(bodyStream);
				
				// The handler for parameters:
				smftp.ParameterHandler += parameter =>
				{
					result.Add(parameter.Name, parameter.Data);
				};
				
				// The parser requires both, the parameter and the file handler:
				smftp.FileHandler += (name, fileName, type, disposition, buffer, bytes) =>
				{
					// Perform a dummy OP:
					dummy = name;
				};
				
				// Start the processing. This call will block until end of processing:
				smftp.Run();
			}
			
			return result;
		}
		
		/// <summary>
		/// Processes the request to get all uploaded files.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="bodyStream">The stream for the request's body.</param>
		/// <param name="tmpFolderPATH">The temporary folder for this request.</param>
		/// <param name="niiRequest">The NoIIS request to register additional open streams.</param>
		/// <returns>Returns the collection with all uploaded files. The collection is possibily empty.</returns>
		public static HttpFileCollectionBase GetFiles(this HttpListenerRequest request, Stream bodyStream, string tmpFolderPATH, NoIISRequest niiRequest)
		{
			if(request == null || tmpFolderPATH == null || bodyStream == null)
			{
				return new NoIISFileCollection();
			}
			
			if(!request.HasEntityBody)
			{
				return new NoIISFileCollection();
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
			
			// If the request's content type does not match the requirements,
			// return here:
			if(!request.ContentType.Contains("multipart/form-data; boundary="))
			{
				return new NoIISFileCollection();
			}
			
			// The cache for all tmp. filenames for files in the request:
			var cacheFilenames = new ConcurrentDictionary<string, NoIISPostedFile>();
			using(bodyStream)
			{
				// Dummy to consume the name of each parameter:
				var dummy = string.Empty;
				
				// Create the parser for the body:
				var smftp = new StreamingMultipartFormDataParser(bodyStream);
				
				// The parser requires both, the parameter and the file handler:
				smftp.ParameterHandler += parameter => dummy = parameter.Name;
				
				// The file handler gets called for every chunk of each file inside:
				smftp.FileHandler += (name, fileName, type, disposition, buffer, bytes) =>
				{
					// Generate a key for this file:
					var key = name + fileName + type + disposition;
					
					// The destination tmp. file name:
					var destination = string.Empty;
					if(!cacheFilenames.ContainsKey(key))
					{
						// Case: A new file.
						cacheFilenames[key] = new NoIISPostedFile();
						
						// Generate the tmp. file name with path:
						cacheFilenames[key].TMPFilenamePATH = tmpFolderPATH + Guid.NewGuid().ToString();
						cacheFilenames[key].FormName = name;
						cacheFilenames[key].setContentType(type);
						cacheFilenames[key].setFileName(fileName);
					}
					
					// Read the current destination:
					destination = cacheFilenames[key].TMPFilenamePATH;
					
					try
					{
						// Append the bytes to the tmp. file:
						using(var fileStream = File.Open(destination, FileMode.Append, FileAccess.Write, FileShare.None))
						{
							fileStream.Write(buffer, 0, bytes);
						}
					}
					catch(Exception e)
					{
						Console.WriteLine("Exception while processing the file upload '{1}' (name='{2}'): {0}", e.Message, fileName, name);
					    Console.WriteLine(e.StackTrace);
					}
				};
				
				try
				{
					// Start the processing. This call will block until end of processing:
					smftp.Run();
				}
				catch(Exception e)
				{
					Console.WriteLine("Exception while processing the file upload: {0}", e.Message);
				    Console.WriteLine(e.StackTrace);
				}
			}
			
			//
			// The files should now be available as tmp. data.
			//
			
			// The result's collection:
			var result = new List<NoIISPostedFile>(cacheFilenames.Count);
			
			// Loop over all files:
			foreach(var key in cacheFilenames.Keys)
			{
				// Get a file:
				var file = cacheFilenames[key];
				
				// Get the tmp. file's name:
				var filename = file.TMPFilenamePATH;
				
				// Get the stream to the file's content:
				var stream = File.OpenRead(filename);
				
				// Register this open stream:
				niiRequest.AddProbablyOpenStream(stream);
				
				// Store the file's length:
				file.setContentLength((int) stream.Length);
				
				// Store the stream:
				file.setInputStream(stream);
				
				// Store this file to the result:
				result.Add(file);
			}
			
			return new NoIISFileCollection(result);
		}
		
		/// <summary>
		/// This method generates the names and values collection for the query string / raw path.
		/// </summary>
		/// <param name="data">The raw path.</param>
		/// <param name="is4FormData">If true, expects not the questionmark '?' within the path!</param>
		/// <returns>Returns the collection of names and values.</returns>
		public static NameValueCollection GetQueryString(this string data, bool is4FormData = false)
		{
			var result = new NameValueCollection();
			if(data == null || data == string.Empty)
			{
				return result;
			}
			
			var elements = data.Split('?');
			var argsRaw = string.Empty;
			
			if(!is4FormData)
			{
				// Case: For GET params (should contain a '?')
				if(elements.Length < 2)
				{
					return result;
				}
				
				argsRaw = elements[1];
			}
			else
			{
				// Case: For form data (should not contain a '?')
				argsRaw = data;
			}
			
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
		
		/// <summary>
		/// Merges two NameValueCollection collections.
		/// </summary>
		/// <param name="collection">The first collection.</param>
		/// <param name="another">The second collection.</param>
		/// <returns>Returns a new NameValueCollection with values and names from both collections.</returns>
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
		
		/// <summary>
		/// Generates the path info for a given URL.
		/// </summary>
		/// <param name="uri">The URL.</param>
		/// <returns>The path info which is possibly empty.</returns>
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
		
		/// <summary>
		/// Generates the file path for a given URL.
		/// </summary>
		/// <param name="uri">The given URL.</param>
		/// <returns>Returns the file path which is possibly empty.</returns>
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
