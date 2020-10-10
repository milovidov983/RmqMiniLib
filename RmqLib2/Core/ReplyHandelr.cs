using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RmqLib2 {
	/// <summary>
	/// Класс отвечает за получение ответов 
	/// от RPC запросов нашего приложения к другим сервисам.
	/// </summary>
	internal class ReplyHandelr : IReplyHandler {
		private readonly ConcurrentDictionary<string, DeliveredMessage> handlers
			= new ConcurrentDictionary<string, DeliveredMessage>();

		public async Task ReceiveReply(object model, BasicDeliverEventArgs ea) {
			var correlationId = ea.BasicProperties.CorrelationId;
			
			var dm = GetCallbackTask(ea, correlationId);

			dm.AppId = ea.BasicProperties.AppId;
			dm.CorrelationId = correlationId;
			dm.Headers = ea.BasicProperties.Headers;
			dm.ReplyTo = ea.BasicProperties.ReplyTo;
			dm.RoutingKey = ea.RoutingKey;
			dm.ResponseTask.SetResult(ea.Body.ToArray());

			RemoveReplySubscription(correlationId);

			await Task.Yield();
		}

		public void AddReplySubscription(string correlationId, DeliveredMessage resonseHandler) {
			handlers.TryAdd(correlationId, resonseHandler);
		}

		public DeliveredMessage RemoveReplySubscription(string correlationId) {
			handlers.TryRemove(correlationId, out var dm);
			return dm;
		}

		private DeliveredMessage GetCallbackTask(BasicDeliverEventArgs ea, string correlationId) {
			if (!handlers.TryGetValue(correlationId, out var dm)) {
				throw new Exception($"Критическая ошибка, в {nameof(handlers)} " +
					$"не найден ни один обработчик для ответа " +
					$"пришедшего из rmq с {nameof(correlationId)}:{correlationId} " +
					$"{nameof(ea.RoutingKey)}:{ea.RoutingKey}");
			}

			return dm;
		}
	}
}
