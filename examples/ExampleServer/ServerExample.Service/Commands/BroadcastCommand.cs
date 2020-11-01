using ServerExample.Service.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SEC = ServerExample.Contracts;

namespace ServerExample.Service.Commands {
	class BroadcastCommand : ReadonlyCommandHandler {
		public BroadcastCommand(Context context) : base(context) {
		}

		protected override async Task ExecuteImpl(RequestContext ctx) {
			var req = ctx.Message.GetContent<SEC.BroadcastCommand.Message>();

			Console.WriteLine($"BroadcastCommand received! Body: {req.Body}");

			await Task.Yield();
		}
	}
}
