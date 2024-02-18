using Grapevine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualAxe.Server
{
	[RestResource]
	public class RestAPI
	{
		private static IRestServer Server;

		public static void ServerStart()
		{
			Server = RestServerBuilder.UseDefaults().Build();
			Server.Prefixes.Add("http://localhost:54321/");
			Server.Start();
		}

		[RestRoute("Get", "/api/IsVisualAxe")]	//HTTPサーバーがVisualAxeのものであることを確認できる
		public async Task IsVisualAxe(IHttpContext context)
		{
			context.Response.StatusCode = 200;
			await context.Response.SendResponseAsync("This server is VisualAxe");
		}

		public static void ServerStop()
		{
			Server.Dispose();
		}
	}
}
