using System;
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
	public static class NoIIS
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
			
			NoIIS.assembly = args[0].Trim();
			NoIIS.tempFolder = args[1].EndsWith(string.Empty + Path.DirectorySeparatorChar) ? args[1] : args[1] + Path.DirectorySeparatorChar;
			NoIIS.maxRequestSizeBytes = int.Parse(args[2]);
			NoIIS.hosts = args.Skip(3).ToArray();
			NoIIS.factories = FindHttpHandlerFactories.findFactories(NoIIS.assembly);
			NoIIS.runner();
		}
		
		/// <summary>
		/// The main-thread of NoIIS where all client requests are arrives.
		/// </summary>
		private static void runner()
		{
			// Set the min. number of threads for the thread-pool:
			ThreadPool.SetMinThreads(100, 100);
			
			// The HTTP listener:
			var listener = new HttpListener();
			
			// Add all hosts to the listener as end-points:
			NoIIS.hosts.ToList().ForEach((n) => {listener.Prefixes.Add(n); Console.WriteLine(n);});
			
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
					if(context.Request.ContentLength64 > NoIIS.maxRequestSizeBytes)
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
							var request = new NoIISRequest(innerContext, NoIIS.tempFolder);
							
							// Create the NoIIS response:
							var response = new NoIISResponse(innerContext);
							
							// Create the NoIIS context with request and response:
							var webContext = new NoIISContext(request, response);
							
							// Search for a handler inside all factories which matches the request:
							var foundHandler = false;
							foreach(var factory in NoIIS.factories)
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
