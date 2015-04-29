using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace NoIIS
{
	public static class NoIIS
	{
		private static string tempFolder = string.Empty;
		private static int maxRequestSizeBytes = 65000;
		private static string[] hosts = new string[0];
		private static string assembly = string.Empty;
		private static IHttpHandlerBaseFactory[] factories = new IHttpHandlerBaseFactory[0];
		
		public static void Main(string[] args)
		{
			if(args.Length < 4)
			{
				Console.WriteLine("Please provide at least three arguments:");
				Console.WriteLine("   1.  The assembly containing the handler factories e.g. 'my-app.dll'");
				Console.WriteLine("   2.  The temp. folder for uploaded files as cache for the processing");
				Console.WriteLine("   3.  The max. request size (bytes)");
				Console.WriteLine("   4+. The prefix(es) for accepted request e.g. 'http://127.0.0.1:8080/' or 'http://*/test/*', etc.");
				Console.WriteLine();
				return;
			}
			
			NoIIS.assembly = args[0].Trim();
			NoIIS.tempFolder = args[1].EndsWith(string.Empty + Path.DirectorySeparatorChar) ? args[0] : args[0] + Path.DirectorySeparatorChar;
			NoIIS.maxRequestSizeBytes = int.Parse(args[2]);
			NoIIS.hosts = args.Skip(3).ToArray();
			NoIIS.factories = FindHttpHandlerFactories.findFactories(NoIIS.assembly);
			NoIIS.runner();
		}
		
		private static void runner()
		{
			var listener = new HttpListener();
			NoIIS.hosts.ToList().ForEach((n) => {listener.Prefixes.Add(n); Console.WriteLine(n);});
			listener.Start();
			
			while(true)
			{
				var context = listener.GetContext();
				if(context.Request.ContentLength64 > NoIIS.maxRequestSizeBytes)
				{
					Console.Write("A request was to huge: {0} bytes.", context.Request.ContentLength64);
					context.Response.Abort();
					continue;
				}
				
				Task.Run(() =>
                {
		         	var innerContext = context;
		         	
				    try
				    {
						var request = new NoIISRequest(innerContext, NoIIS.tempFolder);
						var response = new NoIISResponse(innerContext);
						var webContext = new NoIISContext(request, response);
						var foundHandler = false;
						foreach(var factory in NoIIS.factories)
						{
							var handler = factory.GetHandler(webContext, request.RequestType, request.RawUrl, string.Empty);
							if(handler != null)
							{
								handler.ProcessRequest(webContext);
								foundHandler = true;
								break;
							}
						}
						
						if(!foundHandler)
						{
							Console.WriteLine(string.Format("No handler found for the URL '{0}'.", request.RawUrl));
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
				    	Console.Write(e.StackTrace);
				    	
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
		}
	}
}
