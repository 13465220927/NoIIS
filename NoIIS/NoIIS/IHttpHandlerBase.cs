using System;
using System.Web;

namespace NoIIS
{
	/// <summary>
	/// The exchange-interface for the IHttpHandler of the IIS. This interface is compatible
	/// with the IIS.
	/// </summary>
	public interface IHttpHandlerBase : IHttpHandler
	{
		bool IsReusable
		{
			get;
		}

		void ProcessRequest(HttpContextBase context);
	}
}
