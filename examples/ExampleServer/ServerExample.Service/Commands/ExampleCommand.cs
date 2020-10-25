using ServerExample.Service.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SEC = ServerExample.Contracts;

namespace ServerExample.Service.Commands {
	public class ExampleCommand : ReadonlyCommandHandler {
		public ExampleCommand(Context context) : base(context) {
		}

		protected override async Task ExecuteImpl(RequestContext ctx) {
			var req = ctx.Message.GetContent<SEC.ExampleCommand.Request>();

			Console.WriteLine($"Message get! {req.Message}");

			//throw new Exception("test exept");

			await Hub.SetRpcResultAsync(ctx.Message, new SEC.ExampleCommand.Response { Message = "ServerExample set rpc result 42!" });
		}
	}
}
