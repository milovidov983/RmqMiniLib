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
		private readonly ConcurrentDictionary<string, TaskCompletionSource<DeliveredMessage>> handlers
			= new ConcurrentDictionary<string, TaskCompletionSource<DeliveredMessage>>();

		public async Task ReceiveReply(object model, BasicDeliverEventArgs ea) {
			var correlationId = ea.BasicProperties.CorrelationId;
			
			var taskCompletionSource = GetCallbackTask(ea, correlationId);

			var response = new DeliveredMessage {
				Body = ea.Body.GetString(),
				AppId = ea.BasicProperties.AppId,
				CorrelationId = correlationId,
				Headers = ea.BasicProperties.Headers,
				ReplyTo = ea.BasicProperties.ReplyTo,
				RoutingKey = ea.RoutingKey
			};
			taskCompletionSource.SetResult(response);
			RemoveReplySubscription(correlationId);

			await Task.Yield();
		}

		public void AddReplySubscription(string correlationId, TaskCompletionSource<DeliveredMessage> resonseHandler) {
			handlers.TryAdd(correlationId, resonseHandler);
		}

		public TaskCompletionSource<DeliveredMessage> RemoveReplySubscription(string correlationId) {
			handlers.TryRemove(correlationId, out var task);
			return task;
		}

		private TaskCompletionSource<DeliveredMessage> GetCallbackTask(BasicDeliverEventArgs ea, string correlationId) {
			if (!handlers.TryGetValue(correlationId, out var taskCompletionSource)) {
				throw new Exception($"Критическая ошибка, в {nameof(handlers)} " +
					$"не найден ни один обработчик для ответа " +
					$"пришедшего из rmq с {nameof(correlationId)}:{correlationId} " +
					$"{nameof(ea.RoutingKey)}:{ea.RoutingKey}");
			}

			return taskCompletionSource;
		}
	}
}
