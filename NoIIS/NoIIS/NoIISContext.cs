using System;
using System.Web;
using System.Net;

namespace NoIIS
{
	public class NoIISContext : HttpContextBase
	{
		private NoIISRequest request = null;
		private NoIISResponse response = null;
		
		public NoIISContext(NoIISRequest request, NoIISResponse response)
		{
			this.request = request;
			this.response = response;
		}
		
		public override HttpRequestBase Request
		{
			get
			{
				return this.request;
			}
		}
		
		public override HttpResponseBase Response
		{
			get
			{
				return this.response;
			}
		}
	}
}
