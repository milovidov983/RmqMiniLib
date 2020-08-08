using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	/// <summary>
	/// Класс отвечает за получение ответов 
	/// от RPC запросов нашего приложения к другим сервисам.
	/// </summary>
	internal class ResponseHandelr : IResponseHandelr, IReplyHandler {
		private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> handlers
			= new ConcurrentDictionary<string, TaskCompletionSource<string>>();

		internal ResponseHandelr() {

		}

		public async Task ReceiveReply(object model, BasicDeliverEventArgs ea) {
			var headers = ea.BasicProperties.Headers;
			var correlationId = ea.BasicProperties.CorrelationId;
			var error = new Error(headers);
			var taskCompletionSource = GetCallbackTask(ea, correlationId);
			if (error.HasError) {
				taskCompletionSource.SetException(new RmqException(error));
			} else {
				var response = ea.Body.GetString();
				taskCompletionSource.SetResult(response);
			}
			await Task.Yield();
		}

		public void AddReplySubscription(string correlationId, TaskCompletionSource<string> resonseHandler) {
			handlers.TryAdd(correlationId, resonseHandler);
		}

		public TaskCompletionSource<string> RemoveReplySubscription(string correlationId) {
			handlers.TryRemove(correlationId, out var task);
			return task;
		}

		private TaskCompletionSource<string> GetCallbackTask(BasicDeliverEventArgs ea, string correlationId) {
			if (!handlers.TryGetValue(correlationId, out var taskCompletionSource)) {
				throw new RmqException($"Критическая ошибка, в {nameof(handlers)} " +
					$"не найден ни один обработчик для ответа " +
					$"пришедшего из rmq с {nameof(correlationId)}:{correlationId} " +
					$"{nameof(ea.RoutingKey)}:{ea.RoutingKey}", Error.INTERNAL_ERROR);
			}

			return taskCompletionSource;
		}
	}
}
