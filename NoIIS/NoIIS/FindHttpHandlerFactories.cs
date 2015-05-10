using System;
using System.Reflection;
using System.Collections.Generic;

namespace NoIIS
{
	public static class FindHttpHandlerFactories
	{
		public static IHttpHandlerBaseFactory[] findFactories(string assemblyNameWithPath)
		{
			var result = new List<IHttpHandlerBaseFactory>();
			var ass = Assembly.LoadFrom(assemblyNameWithPath);
			var types = ass.DefinedTypes;
			foreach(var type in types)
			{
				var interfaces = type.GetInterfaces();
				foreach(var iface in interfaces)
				{
					Console.WriteLine("Found interface '{0}' inside of '{1}'.", iface.FullName, type.FullName);
					if(iface.FullName == "NoIIS.IHttpHandlerBaseFactory")
					{
						var constructor = type.GetConstructor(new Type[0]);
						var obj = constructor.Invoke(null);
						result.Add(obj as IHttpHandlerBaseFactory);
						Console.WriteLine("Handler factory found: '{0}'", type.FullName);
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
