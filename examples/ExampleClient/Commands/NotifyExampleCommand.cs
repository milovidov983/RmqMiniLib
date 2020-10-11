using Microsoft.Extensions.Logging;
using RmqLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SC = Server.Contracts;

namespace ExampleClient.Commands {
	public class NotifyExampleCommand : CommandContext, IRmqCommandHandler {
		private readonly ILogger<NotifyExampleCommand> logger;

		public string Topic => SC.NotifyExample.Topic;

		public NotifyExampleCommand(ILogger<NotifyExampleCommand> logger) {
			this.logger = logger;
		}

		public override async Task ExecuteImpl(RequestContext request) {
			var req = request.GetContent<SC.NotifyExample.Message>();
			logger.LogDebug(req.Data);
			await Task.Yield();
		}
	}
}