using System;
using System.Collections.Generic;

namespace RmqLib.Core {
	internal class Initializer {
		private RmqConfig config;
		private IRmqLogger logger;
		public IConnectionManager connectionManager;

		public Initializer(RmqConfig config) {
			this.config = config;

			DeliveryInfo.AppId = config.AppId;
			DeliveryInfo.ExhangeName = config.Exchange;

			var loggerFactory = LoggerFactory.Create("");
			logger = loggerFactory.CreateLogger();
		}

		public void InitConnectionManager() {
			var loggerFactory = LoggerFactory.Create(ConsumerType.Rpc.ToString());
			var channelLogger = loggerFactory.CreateLogger(nameof(ChannelEventsHandler));
			var channelEventsHandlerFactory = ChannelEventsHandlerFactory.Create(channelLogger);

			var channelPoolFactory = ChannelPoolFactory.Create(channelEventsHandlerFactory);

			var responseHandelerLogger = loggerFactory.CreateLogger(nameof(ResponseMessageHandler));
			var responseMessageHandlerFactory = ResponseMessageHandlerFactory.Create(responseHandelerLogger);
			

			connectionManager = new ConnectionManager(
				config, 
				channelPoolFactory,
				responseMessageHandlerFactory
				);

		}

		public IPublisherFactory InitPublisherFactory() {
			return new PublisherFactory(connectionManager, config, logger);
		}

		public SubscriptionManager InitSubscriptions(IRabbitHub hub, Dictionary<string, IRabbitCommand> commandHandlers) {
			SubscriptionManager subscriptionManager = null;
			try {
				var topicManageChannel = connectionManager.GetSubscriptionChannel();

				IConsumerBinder topicListenerBinder = new TopicListinerConsumerBinder(config.Queue);
				IConsumerFactory topicListenerConsumerFactory = new ConsumerFactory(topicManageChannel, topicListenerBinder);


				var loggerFactory = LoggerFactory.Create(ConsumerType.Subs.ToString());
				var managerLogger = loggerFactory.CreateLogger(nameof(ConsumerManager));

				IConsumerManager topicListenerConsumerManager = new ConsumerManager(topicListenerConsumerFactory, managerLogger);
				topicListenerConsumerManager.InitConsumer();

				IMainConsumerEventHandlerFactory topicListenerConsumerEventHandlersFactory
					= ConsumerEventHandlersFactory.Create(topicListenerConsumerManager, loggerFactory);

				var topicListenerConsumerEventHandlers = topicListenerConsumerEventHandlersFactory.CreateMainHandler();

				subscriptionManager = new SubscriptionManager(
					topicManageChannel,
					hub,
					commandHandlers,
					config);

				topicListenerConsumerEventHandlers.AddReceiveHandler(subscriptionManager.Handler);

			} catch (Exception e) {
					throw new InvalidOperationException(
						$"Cant create {nameof(SubscriptionManager)} " +
						$"Error: {e.Message} ", e);

			}
			return subscriptionManager;
		}
	}
}
