using System;
using System.Reflection;
using System.Collections.Generic;

namespace NoIIS
{
	/// <summary>
	/// This class contains the logic about how to find factories for HTTP handlers. You dont have to
	/// use this class by your own, NoIIS will handle this for you.
	/// </summary>
	public static class FindHttpHandlerFactories
	{
		/// <summary>
		/// This method searches for factories at the given assembly.
		/// </summary>
		/// <param name="assemblyNameWithPath">The path and name of the assembly.</param>
		/// <returns>Returns an array with all found factories.</returns>
		public static IHttpHandlerBaseFactory[] findFactories(string assemblyNameWithPath)
		{
			// A list for all the factories:
			var result = new List<IHttpHandlerBaseFactory>();
			
			// Load the assembly:
			var ass = Assembly.LoadFrom(assemblyNameWithPath);
			
			// Loop over all types inside the assembly:
			var types = ass.DefinedTypes;
			foreach(var type in types)
			{
				// Get all interfaces for this type:
				var interfaces = type.GetInterfaces();
				
				// Loop over all interfaces:
				foreach(var iface in interfaces)
				{
					Console.WriteLine("Found interface '{0}' inside of '{1}'.", iface.FullName, type.FullName);
					
					// Found a matching factory:
					if(iface.FullName == "NoIIS.IHttpHandlerBaseFactory")
					{
						// Get the default-constructor for this factory:
						var constructor = type.GetConstructor(new Type[0]);
						
						// Call the default-constructor:
						var obj = constructor.Invoke(null);
						
						// Add this factory to the result:
						result.Add(obj as IHttpHandlerBaseFactory);
						Console.WriteLine("Handler factory found: '{0}'", type.FullName);
						
						// More than one factory per type is not possible:
						break;
					}
				}
			}
			
			Console.WriteLine("Found {0} handler factories.", result.Count);
			Console.WriteLine();
			return result.ToArray();
		}
	}
}
