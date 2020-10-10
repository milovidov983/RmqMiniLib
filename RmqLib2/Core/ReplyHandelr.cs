﻿using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RmqLib2 {
	/// <summary>
	/// Класс отвечает за получение ответов 
	/// от RPC запросов нашего приложения к другим сервисам.
	/// </summary>
	internal class ReplyHandelr : IReplyHandler {
		private readonly ConcurrentDictionary<string, ResponseMessage> handlers
			= new ConcurrentDictionary<string, ResponseMessage>();

		public Task ReceiveReply(object model, BasicDeliverEventArgs ea) {
			Console.WriteLine("Get response !");
			var correlationId = ea.BasicProperties.CorrelationId;
			var dm = GetCallbackTask(ea, correlationId);

			dm.AppId = ea.BasicProperties.AppId;
			dm.CorrelationId = correlationId;
			dm.Headers = ea.BasicProperties.Headers;
			dm.ReplyTo = ea.BasicProperties.ReplyTo;
			dm.RoutingKey = ea.RoutingKey;
			dm.ResponseTask.SetResult(ea.Body.ToArray());

			RemoveReplySubscription(correlationId);

			return Task.CompletedTask;
		}

		public void AddReplySubscription(string correlationId, ResponseMessage resonseHandler) {
			handlers.TryAdd(correlationId, resonseHandler);
		}

		public ResponseMessage RemoveReplySubscription(string correlationId) {
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
			}

			return dm;
		}
	}
}
