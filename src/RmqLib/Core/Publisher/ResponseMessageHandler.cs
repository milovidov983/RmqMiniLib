using RabbitMQ.Client.Events;
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

		public Task HandleMessage(object model, BasicDeliverEventArgs ea) {
			var correlationId = ea.BasicProperties.CorrelationId;
			try {
				var dm = GetCallbackTask(ea, correlationId);

				if(dm is null) {
					return Task.CompletedTask;
				}

				dm.AppId = ea.BasicProperties.AppId;
				dm.CorrelationId = correlationId;
				dm.Headers = ea.BasicProperties.Headers;
				dm.ReplyTo = ea.BasicProperties.ReplyTo;
				dm.RoutingKey = ea.RoutingKey;
				dm.ResponseTask.SetResult(ea.Body.ToArray());


				return Task.CompletedTask;
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
				//throw new Exception($"Критическая ошибка, в {nameof(handlers)} " +
				//	$"не найден ни один обработчик для ответа " +
				//	$"пришедшего из rmq с {nameof(correlationId)}:{correlationId} " +
				//	$"{nameof(ea.RoutingKey)}:{ea.RoutingKey}");
				Console.WriteLine($"Критическая ошибка, " +
					$"не найден ни один обработчик для ответа " +
					$"пришедшего из rmq с {nameof(correlationId)}:{correlationId} " +
					$"{nameof(ea.RoutingKey)}:{ea.RoutingKey}");

				Console.WriteLine("Ответ получать некому поэтому мы его просто дропаем");

			}

			return dm;
		}
	}


	class ResponseMessageHandlerFactory : IResponseMessageHandlerFactory {
		IResponseMessageHandler responseMessageHandler = new ResponseMessageHandler();
		public IResponseMessageHandler GetHandler() {
			return responseMessageHandler;
		}

		public static IResponseMessageHandlerFactory Create() {
			return new ResponseMessageHandlerFactory();
		}
	}

	interface IResponseMessageHandlerFactory {
		IResponseMessageHandler GetHandler();
	}
}
