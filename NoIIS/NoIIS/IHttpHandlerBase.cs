using System;
using System.Web;

namespace NoIIS
{
	public interface IHttpHandlerBase : IHttpHandler
	{
		bool IsReusable
		{
			get;
		}

		void ProcessRequest(HttpContextBase context);
	}
}
