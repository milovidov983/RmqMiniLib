using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RmqLib {
	/// <summary>
	/// Класс отвечает за получение ответов 
	/// от RPC запросов нашего приложения к другим сервисам.
	/// </summary>
	internal class ResponseMessageHandler : IResponseMessageHandler {
		private readonly ConcurrentDictionary<string, ResponseMessage> handlers
			= new ConcurrentDictionary<string, ResponseMessage>();

		private readonly IRmqLogger logger;

		public ResponseMessageHandler(IRmqLogger logger) {
			this.logger = logger;
		}

		public void HandleMessage(object model, BasicDeliverEventArgs ea) {
			var correlationId = ea.BasicProperties.CorrelationId;
			//logger.Debug($"[{nameof(ResponseMessageHandler)}] handle msg with correlation id: {correlationId}");
			try {
				var dm = GetCallbackTask(ea, correlationId);

				if(dm is null) {
					return;
				}

				dm.AppId = ea.BasicProperties.AppId;
				dm.CorrelationId = correlationId;
				dm.Headers = ea.BasicProperties.Headers;
				dm.ReplyTo = ea.BasicProperties.ReplyTo;
				dm.RoutingKey = ea.RoutingKey;


				// TODO передавать ReadOnlyMemory
				// TResponse response = JsonSerializer.Deserialize<TResponse>(Body.Span);

				dm.ResponseTask.SetResult(ea.Body);

			} finally {
				RemoveSubscription(correlationId);
			}
		}

		public void AddSubscription(string correlationId, ResponseMessage responseHandler) {
			//logger.Debug($"[{nameof(ResponseMessageHandler)}] AddSubscription with correlation id: {correlationId}");
			handlers.TryAdd(correlationId, responseHandler);
		}

		public ResponseMessage RemoveSubscription(string correlationId) {
			//logger.Debug($"[{nameof(ResponseMessageHandler)}] RemoveSubscription with correlation id: {correlationId}");
			handlers.TryRemove(correlationId, out var dm);
			return dm;
		}

		private ResponseMessage GetCallbackTask(BasicDeliverEventArgs ea, string correlationId) {
			if (!handlers.TryGetValue(correlationId, out var dm)) {
				//throw new Exception($"Критическая ошибка, в {nameof(handlers)} " +
				//	$"не найден ни один обработчик для ответа " +
				//	$"пришедшего из rmq с {nameof(correlationId)}:{correlationId} " +
				//	$"{nameof(ea.RoutingKey)}:{ea.RoutingKey}");
				logger.Error($"[{nameof(ResponseMessageHandler)}] Критическая ошибка, " +
					$"не найден ни один обработчик для ответа " +
					$"пришедшего из rmq с {nameof(correlationId)}:{correlationId} " +
					$"{nameof(ea.RoutingKey)}:{ea.RoutingKey}");

				logger.Error("[{nameof(ResponseMessageHandler)}] Ответ получать некому поэтому мы его просто дропаем");

			}

			return dm;
		}
	}
}
