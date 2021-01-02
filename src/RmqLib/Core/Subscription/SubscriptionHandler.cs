using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib.Core {

	internal class SubscriptionHandler : ISubscriptionHandler {
		private readonly IChannelWrapper wrappedChannel;

		private readonly ConcurrentDictionary<string, IRabbitCommand> topicHandlers
			= new ConcurrentDictionary<string, IRabbitCommand>();

		private QueueHandlersConfig queueHandlersConfig;

		private readonly IRmqLogger logger;

		public SubscriptionHandler(
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

		public void Handle(object model, BasicDeliverEventArgs ea) {
			var routingKey = ea.RoutingKey;
			var dt = ea.DeliveryTag;
			var deliveredMessage = ea.CreateDeliveredMessage();
			Task.Run(async () => {
				try {
					var acceptMessage =  await RunBeforeExecuteHandler(deliveredMessage);
					if (!acceptMessage) {
						await Ask(dt, MessageProcessResult.Reject);
						return;
					}

					var handlerExists = topicHandlers.TryGetValue(routingKey, out var handler);
					if (!handlerExists) {
						MessageProcessResult mpr = await ExecuteUnexpectedTopicHandler(deliveredMessage);
						await Ask(dt, mpr);
						return;
					}


					var processResult = MessageProcessResult.Reject;
					try {
						processResult = await handler.Execute(deliveredMessage);
					} catch (Exception e) {
						processResult = await ExecuteExceptionHandler(e, deliveredMessage);
					} finally {
						processResult = await RunAfterExecuteHandler(deliveredMessage, processResult);
						await Ask(dt, processResult);
					}

				} catch (Exception e) {
					logger.Error($"{nameof(SubscriptionHandler)}.{nameof(Handle)} {e.Message} {e.InnerException?.Message}");
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
			return Task.FromResult(MessageProcessResult.Reject);
		}

		private async Task<MessageProcessResult> ExecuteExceptionHandler(Exception e, DeliveredMessage deliveredMessage) {
			var processResult = MessageProcessResult.Ack;
			if (queueHandlersConfig.OnExceptionHandler != null) {
				var handleResultFlag = await queueHandlersConfig.OnExceptionHandler(e, deliveredMessage);

				processResult = handleResultFlag 
					? MessageProcessResult.Ack 
					: MessageProcessResult.Reject;
			}
			return processResult;
		}

		private Task<MessageProcessResult> RunAfterExecuteHandler(
			DeliveredMessage deliveredMessage,
			MessageProcessResult processResult) {

			if (queueHandlersConfig.AfterExecuteHandler != null) {
				return queueHandlersConfig.AfterExecuteHandler(deliveredMessage, processResult);
			}

			return Task.FromResult(processResult);
		}

		private Task<bool> RunBeforeExecuteHandler(DeliveredMessage deliveredMessage) {
			if (queueHandlersConfig.BeforeExecuteHandler != null) {
				return queueHandlersConfig.BeforeExecuteHandler(deliveredMessage);
			}
			return Task.FromResult(true);
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
