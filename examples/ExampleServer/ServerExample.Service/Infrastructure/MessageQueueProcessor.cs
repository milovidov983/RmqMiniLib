
using RmqLib;
using System;
using System.Threading.Tasks;
using CMCI = Common.Contracts.Infrastructure;
using SEC = ServerExample.Contracts;
namespace ServerExample.Service.Infrastructure {
	public class MessageQueueProcessor {
		private readonly IRabbitHub hub;
		private readonly Settings settings;
		private readonly ILogger logger;
		private IDisposable subscriptions;

		public MessageQueueProcessor(IRabbitHub hub, Settings settings, ILogger logger) {
			this.hub = hub;
			this.settings = settings;
			this.logger = logger;
		}

		public async Task Start() {
			var context = new Context(hub, logger);

			subscriptions = await hub
				.DefineHandlers()
				.AddHandlers(
					cfg =>
						cfg
							.OnException((exc, dm) => {
								logger.Fatal(exc, "Error on MessageProcessor.");

								if (dm.IsRpcMessage()) {
									hub.SetRpcErrorAsync(dm, exc.Message, (int)CMCI.StatusCodes.InternalError);
								}

								return Task.FromResult(false);
							})
							.OnUnexpectedTopic(async dm => {
								var message = $"{dm.GetTopic()} was unexpected";
								logger.Warn(message);


								if (dm.IsRpcMessage()) {
									await hub.SetRpcErrorAsync(dm, message, (int)CMCI.StatusCodes.InvalidRequest);
								}

								return await MessageProcessResult.RejectTask;
							})
							.OnTopic(SEC.ExampleCommand.Topic, new Commands.ExampleCommand(context))
				)
				.Start();
		}

		public void Stop() {
			subscriptions?.Dispose();
		}
	}
}
