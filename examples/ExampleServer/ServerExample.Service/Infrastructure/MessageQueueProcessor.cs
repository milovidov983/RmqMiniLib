
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
			
			// Определение подписчиков на топики rabbitMq
			subscriptions = await hub
				.DefineHandlers()
				.AddHandlers(
					cfg =>
						cfg
							// Перехватчик исключений вызванных выполнением команд обработчиков топиков
							.OnException((exc, dm) => {
								logger.Fatal(exc, "Error on MessageProcessor.");

								if (dm.IsRpcMessage()) {
									hub.SetRpcErrorAsync(dm, exc.Message, (int)CMCI.StatusCodes.InternalError);
								}

								return Task.FromResult(false);
							})
							// Перехватчик сообщений с топиком на которые нет обработчика
							.OnUnexpectedTopic(async dm => {
								var message = $"{dm.GetTopic()} was unexpected";
								logger.Warn(message);


								if (dm.IsRpcMessage()) {
									await hub.SetRpcErrorAsync(dm, message, (int)CMCI.StatusCodes.InvalidRequest);
								}

								return await MessageProcessResult.RejectTask;
							})
							// Регистрация обработчика топика, 1. Аргумент топик rabbitMq 2. Команда обработчик
							.OnTopic(SEC.ExampleCommand.Topic, new Commands.ExampleCommand(context))
							.OnTopic(SEC.BroadcastCommand.Topic, new Commands.BroadcastCommand(context))
				)
				.Start();
		}

		public void Stop() {
			subscriptions?.Dispose();
		}
	}
}
