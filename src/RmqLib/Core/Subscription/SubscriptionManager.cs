using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib.Core {

	internal class SubscriptionManager : ISubscriptionManager {
		private readonly IChannelWrapper wrappedChannel;
		private readonly ConcurrentDictionary<string, IRabbitCommand> topicHandlers
			= new ConcurrentDictionary<string, IRabbitCommand>();

		private QueueHandlersConfig queueHandlersConfig;

		private readonly IRmqLogger logger;

		public SubscriptionManager(
			IChannelWrapper wrappedChannel,
			IRabbitHub hub,
			Dictionary<string, IRabbitCommand> commandHandlers,
			RmqConfig config,
			IRmqLogger logger) {

			this.wrappedChannel = wrappedChannel;
			this.logger = logger;

			foreach (var handler in commandHandlers) {
				wrappedChannel.QueueBind(queue: config.Queue,
								exchange: config.Exchange,
								routingKey: handler.Key);

				topicHandlers.TryAdd(handler.Key, handler.Value);
				handler.Value.WithHub(hub);
			}

		}

		public void Handler(object model, BasicDeliverEventArgs ea) {
			var routingKey = ea.RoutingKey;
			var dt = ea.DeliveryTag;
			DeliveredMessage deliveredMessage = CreateDeliveredMessage(ea);
			Task.Factory.StartNew(async () => {
				try {
					await ExecuteBeforeExecuteHandler(deliveredMessage);

					var handlerExists = topicHandlers.TryGetValue(routingKey, out var handler);
					if (!handlerExists) {
						MessageProcessResult mpr = await ExecuteUnexpectedTopicHandler(deliveredMessage);
						await Ask(dt, mpr);
						return;
					}


					MessageProcessResult processResult = MessageProcessResult.Reject;
					try {
						processResult = await handler.Execute(deliveredMessage);
					} catch (Exception e) {
						await ExecuteExceptionHandler(e, deliveredMessage);
					} finally {
						processResult = await ExecuteAfterExecuteHandler(deliveredMessage, processResult);
						await Ask(dt, processResult);
					}

				} catch (Exception e) {
					logger.Error($"{nameof(SubscriptionManager)}.{nameof(Handler)} {e.Message}");
					await wrappedChannel.BasicReject(dt, false);
				}
			});
		}

		internal void AddHandler(QueueHandlersConfig queueHandlersConfig) {
			this.queueHandlersConfig = queueHandlersConfig;
		}

		private Task<MessageProcessResult> ExecuteUnexpectedTopicHandler(DeliveredMessage deliveredMessage) {
			if (queueHandlersConfig.OnUnexpectedTopicHandler != null) {
				return queueHandlersConfig.OnUnexpectedTopicHandler(deliveredMessage);
			}
			return Task.FromResult(MessageProcessResult.Ack);
		}



		private Task ExecuteExceptionHandler(Exception e, DeliveredMessage deliveredMessage) {
			if (queueHandlersConfig.OnExceptionHandler != null) {
				return queueHandlersConfig.OnExceptionHandler(e, deliveredMessage);
			}
			return Task.CompletedTask;
		}

		private Task<MessageProcessResult> ExecuteAfterExecuteHandler(
			DeliveredMessage deliveredMessage,
			MessageProcessResult processResult) {

			if (queueHandlersConfig.AfterExecuteHandler != null) {
				return queueHandlersConfig.AfterExecuteHandler(deliveredMessage, processResult);
			}
			return Task.FromResult(MessageProcessResult.Ack);
		}

		private Task ExecuteBeforeExecuteHandler(DeliveredMessage deliveredMessage) {
			if (queueHandlersConfig.BeforeExecuteHandler != null) {
				return queueHandlersConfig.BeforeExecuteHandler(deliveredMessage);
			}
			return Task.CompletedTask;
		}

		private DeliveredMessage CreateDeliveredMessage(BasicDeliverEventArgs ea) {
			var body = ea.Body;
			var props = ea.BasicProperties;
			var routingKey = ea.RoutingKey;
			var dt = ea.DeliveryTag;
			return new DeliveredMessage(props, routingKey, body, dt);
		}

		private async Task Ask(ulong dt, MessageProcessResult processResult) {
			switch (processResult) {
				case MessageProcessResult _ when processResult == MessageProcessResult.Ack:
					await wrappedChannel.BasicAck(deliveryTag: dt, multiple: false);
					break;
				case MessageProcessResult _ when processResult == MessageProcessResult.Requeue:
					await wrappedChannel.BasicReject(dt, true);
					break;
				case MessageProcessResult _ when processResult == MessageProcessResult.Reject:
					await wrappedChannel.BasicReject(dt, false);
					break;
			}

		}
	}
}
