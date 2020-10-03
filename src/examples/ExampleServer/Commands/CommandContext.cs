using RmqLib;
using Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SC = Server.Contracts;

namespace ExampleServer.Commands {

	public abstract class CommandContext {
		protected IRmqSender hub;
		public async Task<MessageProcessResult> Execute(RequestContext request) {
			hub = request.Hub;
			try {
				await ExecuteImpl(request);
				return MessageProcessResult.Ack;
			} catch (RmqException e) {
					//logError
				await LogError(request, e);
			}
			return MessageProcessResult.Reject;

		}

		private async Task LogError(RequestContext request, RmqException e) {
			try {
				Console.WriteLine(nameof(CommandContext) + " ERROR! : ");
				Console.WriteLine(e.Message);

				if (request.IsRpcMessage()) {
					await hub.SetRpcErrorAsync(request, e);
				}
			} catch { }
		}

		public abstract Task ExecuteImpl(RequestContext message);


	}
}
