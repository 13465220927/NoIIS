using System;
using System.Web;

namespace NoIIS
{
	/// <summary>
	/// The exchange-interface for the IHttpHandlerFactory of the IIS. This interface is compatible
	/// to the IIS.
	/// </summary>
	public interface IHttpHandlerBaseFactory : IHttpHandlerFactory
	{
		IHttpHandlerBase GetHandler(HttpContextBase context, string requestType, string url, string pathTranslated);
		void ReleaseHandler(IHttpHandlerBase handler);
	}
}
