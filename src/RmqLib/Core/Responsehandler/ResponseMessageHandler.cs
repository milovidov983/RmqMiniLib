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

				dm.ResponseTask.SetResult(ea.Body);

			} finally {
				RemoveSubscription(correlationId);
			}
		}

		public void AddSubscription(string correlationId, ResponseMessage responseHandler) {
			handlers.TryAdd(correlationId, responseHandler);
		}

		public ResponseMessage RemoveSubscription(string correlationId) {
			handlers.TryRemove(correlationId, out var dm);
			return dm;
		}

		private ResponseMessage GetCallbackTask(BasicDeliverEventArgs ea, string correlationId) {
			if (!handlers.TryGetValue(correlationId, out var dm)) {
				logger.Warn($"В пришедшем ответе указан неизвестный {nameof(correlationId)}" +
					$" для данного ID нет ожидающей его задачи " +
					$"{nameof(ea.RoutingKey)}:{ea.RoutingKey}");
			}

			return dm;
		}
	}
}
