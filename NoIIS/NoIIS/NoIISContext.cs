using System;
using System.Web;

namespace NoIIS
{
	/// <summary>
	/// This class is the web context which contains the request and response for a client's request.
	/// Consider this class as not thread-safe and use an instance only at one thread after another.
	/// You will not create an instance by your own, NoIIS will handle this for you.
	/// </summary>
	public class NoIISContext : HttpContextBase
	{
		private NoIISRequest request = null;
		private NoIISResponse response = null;
		
		/// <summary>
		/// The constructor of this class. You dont have to use this by your own, NoIIS will handle this for you.
		/// </summary>
		/// <param name="request">The client's request.</param>
		/// <param name="response">Your response to the client's request.</param>
		public NoIISContext(NoIISRequest request, NoIISResponse response)
		{
			this.request = request;
			this.response = response;
		}
		
		/// <summary>
		/// Gets the client's request.
		/// </summary>
		public override HttpRequestBase Request
		{
			get
			{
				return this.request;
			}
		}
		
		/// <summary>
		/// Gets the response to the client's request.
		/// </summary>
		public override HttpResponseBase Response
		{
			get
			{
				return this.response;
			}
		}
	}
}
