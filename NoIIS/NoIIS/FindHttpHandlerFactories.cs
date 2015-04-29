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
					Console.WriteLine(string.Format("Found interface '{0}' inside of '{1}'.", iface.FullName, type.FullName));
					if(iface.FullName == "NoIIS.IHttpHandlerBaseFactory")
					{
						var constructor = type.GetConstructor(new Type[0]);
						var obj = constructor.Invoke(null);
						result.Add(obj as IHttpHandlerBaseFactory);
						Console.WriteLine(string.Format("Handler factory found: '{0}'", type.FullName));
						break;
					}
				}
			}
			
			Console.WriteLine(string.Format("Found {0} handler factories.", result.Count));
			Console.WriteLine();
			return result.ToArray();
		}
	}
}
