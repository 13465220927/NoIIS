using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace NoIIS
{
	/// <summary>
	/// This main class for the NoIIS web server. You dont need to use this class inside your projects while development.
	/// Instead, this class provides the Main method for the web server, which you will use at the deploy time.
	/// </summary>
	public static class NoIISServer
	{
		// The temporary folder for e.g. file uploads of clients, etc.
		private static string tempFolder = string.Empty;
		
		// The max. request size we will accept:
		private static int maxRequestSizeBytes = 65000;
		
		// The hosts for this web server e.g. http://127.0.0.1:80 etc.
		private static string[] hosts = new string[0];
		
		// The assembly which contains your business logic:
		private static string assembly = string.Empty;
		
		// All found handler factories of your assembly:
		private static IHttpHandlerBaseFactory[] factories = new IHttpHandlerBaseFactory[0];
		
		/// <summary>
		/// The entry point for the web server. Gets called after you start NoIIS.
		/// </summary>
		/// <param name="args">The parameters you give to NoIIS.</param>
		public static void Main(string[] args)
		{
			if(args.Length < 4)
			{
				Console.WriteLine("Please provide at least four arguments:");
				Console.WriteLine("   1.  The assembly containing the handler factories e.g. 'my-app.dll'");
				Console.WriteLine("   2.  The temp. folder for uploaded files as cache for the processing");
				Console.WriteLine("   3.  The max. request size (bytes)");
				Console.WriteLine("   4+. The prefix(es) for accepted request e.g. 'http://127.0.0.1:8080/' or 'http://*/test/*', etc.");
				Console.WriteLine();
				return;
			}
			
			NoIISServer.assembly = args[0].Trim();
			NoIISServer.tempFolder = args[1].EndsWith(string.Empty + Path.DirectorySeparatorChar) ? args[1] : args[1] + Path.DirectorySeparatorChar;
			NoIISServer.maxRequestSizeBytes = int.Parse(args[2]);
			NoIISServer.hosts = args.Skip(3).ToArray();
			NoIISServer.factories = FindHttpHandlerFactories.findFactories(NoIISServer.assembly);
			NoIISServer.runner();
		}

		/// <summary>
		/// Setup the NoIIS envirnonment instead of using the NoIIS.exe. After setup, please call runner() to start the server.
		/// </summary>
		/// <param name="factories">Your factories which provides all of your handlers. You must provide at least one factory in order to use NoIIS.</param>
		/// <param name="tempFolderPath">The already existing temporary directory for e.g. file uploads, etc. If you use an empty string, the server will create a directory at the current working directory.</param>
		/// <param name="maxRequestSizeBytes">The max. request size (bytes) for every request. This parameter limits e.g. file uploads. Default: approx. 5 MB</param>
		/// <param name="host">The host and port where the web server will receive requests. Default: http://127.0.0.1:50000/</param>
		/// <param name="hosts">Instead of using one host, you can provide here several hosts. If you use this parameter, NoIIS will ignore the host parameter.</param>
		public static void setup(IEnumerable<IHttpHandlerBaseFactory> factories, string tempFolderPath = "", int maxRequestSizeBytes = 5000000, string host = "http://127.0.0.1:50000/", IEnumerable<string> hosts = null)
		{
			// Store the factories:
			NoIISServer.factories = factories.ToArray();
			
			// Store the max. size:
			NoIISServer.maxRequestSizeBytes = maxRequestSizeBytes;
			
			// Store the host or hosts:
			NoIISServer.hosts = hosts != null ? hosts.ToArray() : new string[] { host };
			
			// Provides a temp. folder?
			if(tempFolderPath == null || tempFolderPath == string.Empty)
			{
				// Case: No path provided!
				var currentWorkingDIR = Environment.CurrentDirectory;
				NoIISServer.tempFolder = currentWorkingDIR.EndsWith(string.Empty + Path.DirectorySeparatorChar) ? currentWorkingDIR : currentWorkingDIR + Path.DirectorySeparatorChar;
				
				try
				{
					Directory.CreateDirectory(NoIISServer.tempFolder);
				}
				catch
				{
				}
			} else {
				// Case: Path was provided!
				NoIISServer.tempFolder = tempFolderPath.EndsWith(string.Empty + Path.DirectorySeparatorChar) ? tempFolderPath : tempFolderPath + Path.DirectorySeparatorChar;
			}
		}
		
		/// <summary>
		/// The main-thread of NoIIS where all client requests are arrives.
		/// </summary>
		public static void runner()
		{
			// Set the min. number of threads for the thread-pool:
			ThreadPool.SetMinThreads(100, 100);
			
			// The HTTP listener:
			var listener = new HttpListener();
			
			// Add all hosts to the listener as end-points:
			NoIISServer.hosts.ToList().ForEach((n) => {listener.Prefixes.Add(n); Console.WriteLine(n);});
			
			// Start listening:
			listener.Start();
			
			// Loop forever:
			while(true)
			{
				try
				{
					// Will wait for the next client request: 
					var context = listener.GetContext();
					
					// Filter-out any requests which are to huge:
					if(context.Request.ContentLength64 > NoIISServer.maxRequestSizeBytes)
					{
						Console.WriteLine("A request was to huge: {0} bytes.", context.Request.ContentLength64);
						context.Response.Abort();
						continue;
					}
					
					// Any further step will be processed as own thread. This enables the web server
					// to accept the next request.
					Task.Run(() =>
	                {
					    // Copy the context in the thread:
			         	var innerContext = context;
			         	
					    try
					    {
					    	// Create the NoIIS request:
							var request = new NoIISRequest(innerContext, NoIISServer.tempFolder);
							
							// Create the NoIIS response:
							var response = new NoIISResponse(innerContext);
							
							// Create the NoIIS context with request and response:
							var webContext = new NoIISContext(request, response);
							
							// Search for a handler inside all factories which matches the request:
							var foundHandler = false;
							foreach(var factory in NoIISServer.factories)
							{
								// Does this factory is able to deliver a handler for this request?
								var handler = factory.GetHandler(webContext, request.RequestType, request.Path, string.Empty);
								
								// Case: Yes, we have found the first handler:
								if(handler != null)
								{
									// Let the handler process the request:
									handler.ProcessRequest(webContext);
									foundHandler = true;
									
									// We only use the first matching handler:
									break;
								}
							}
							
							// Case: No handler was found
							if(!foundHandler)
							{
								Console.WriteLine("No handler found for the URL '{0}'.", request.RawUrl);
								response.StatusCode = 404;
							}
							
							try
							{
								response.Dispose();
							}
							catch
							{
							}
							
							try
							{
								request.Dispose();
							}
							catch
							{
							}
					    }
					    catch(Exception e)
					    {
					    	Console.WriteLine("Exception while processing request: {0}", e.Message);
					    	Console.WriteLine(e.StackTrace);
					    	
					    	try
					    	{
					    		innerContext.Response.Abort();
					    	}
					    	catch
					    	{
					    	}
					    }
					    
	             	});
				}
				catch(Exception e)
				{
					Console.WriteLine("Exception while accepting request: {0}", e.Message);
					Console.WriteLine(e.StackTrace);
				}
	        }
		}
	}
}
