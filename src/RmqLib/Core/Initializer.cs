using System;

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
			var channelEventsHandlerFactory = ChannelEventsHandlerFactory.Create(logger);
			var channelPoolFactory = ChannelPoolFactory.Create(channelEventsHandlerFactory);
			var responseMessageHandlerFactory = ResponseMessageHandlerFactory.Create(logger);
			
			connectionManager = new ConnectionManager(
				config, 
				channelPoolFactory,
				responseMessageHandlerFactory,
				logger);

		}

		public IPublisherFactory InitPublisherFactory() {
			return new PublisherFactory(connectionManager);
		}

		public SubscriptionManager InitSubscriptions(IRabbitHub hub) {
			SubscriptionManager subscription = null;
			try {
				CommandHandler[] subscriptions = Array.Empty<CommandHandler>();

				connectionManager.CreateSubscriptionChannelPool(config.PrefetchCount);
				var topicManageChannel = connectionManager.GetSubscriptionChannel();

				IConsumerBinder topicListenerBinder = new TopicListinerConsumerBinder(config.Queue);
				IConsumerFactory topicListenerConsumerFactory = new ConsumerFactory(topicManageChannel, topicListenerBinder);


				var loggerFactory = LoggerFactory.Create(ConsumerType.Subs.ToString());
				var managerLogger = loggerFactory.CreateLogger(nameof(ConsumerManager));

				IConsumerManager topicListenerConsumerManager = new ConsumerManager(topicListenerConsumerFactory, managerLogger);


				IMainConsumerEventHandlerFactory topicListenerConsumerEventHandlersFactory
					= ConsumerEventHandlersFactory.Create(topicListenerConsumerManager, loggerFactory);

				var topicListenerConsumerEventHandlers = topicListenerConsumerEventHandlersFactory.CreateMainHandler();

				ISubscriptionManager subscriptionManager = new SubscriptionManager(
					topicManageChannel,
					hub,
					subscriptions,
					config);

				topicListenerConsumerEventHandlers.AddHandler(subscriptionManager.Handler);

			} catch (Exception e) {
					throw new InvalidOperationException(
						$"Cant create {nameof(SubscriptionManager)} " +
						$"Error: {e.Message} ", e);

			}
			return subscription;
		}
	}
}
