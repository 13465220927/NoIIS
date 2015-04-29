using System;
using System.Web;

namespace NoIIS
{
	public interface IHttpHandlerBaseFactory : IHttpHandlerFactory
	{
		IHttpHandlerBase GetHandler(HttpContextBase context, string requestType, string url, string pathTranslated);
		void ReleaseHandler(IHttpHandlerBase handler);
	}
}
