using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace RmqLib.Core {
	internal class Initializer {
		private RmqConfig config;
		private IRmqLogger logger;
		public IConnectionManager connectionManager;

		public Initializer(RmqConfig config, ILogger externalLogger = null) {
			this.config = config;

			DeliveryInfo.AppId = config.AppId;
			DeliveryInfo.ExhangeName = config.Exchange;

			if(externalLogger != null) {
				LoggerFactory.SetupExternalLogger(externalLogger);
			}


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

		public SubscriptionHandler InitSubscriptions(IRabbitHub hub, Dictionary<string, IRabbitCommand> commandHandlers, QueueHandlersConfig handlersConfig) {
			SubscriptionHandler subscriptionHandler = null;
			try {
				IChannelWrapper subscriptionWrapChannel = connectionManager.GetSubscriptionChannel();

				IConsumerBinder subscriptionConsumerBinder = new SubsciptionConsumerBinder(config.Queue);
				IConsumerFactory subscriptionConsumerFactory = new ConsumerFactory(subscriptionWrapChannel, subscriptionConsumerBinder);

				var consumerManagerFactory = ConsumerManagerFactory.Create(ConsumerType.Subs);


				IConsumerManager subscriptionConsumerManager 
					= consumerManagerFactory.CreateConsumerManager(subscriptionConsumerFactory);


				var loggerFactory = LoggerFactory.Create(ConsumerType.Subs.ToString());
				IMainConsumerEventHandlerFactory topicListenerConsumerEventHandlersFactory
					= ConsumerEventHandlersFactory.Create(subscriptionConsumerManager, loggerFactory);

				IConsumerMainEventHandlers topicListenerConsumerEventHandlers 
					= topicListenerConsumerEventHandlersFactory.CreateMainHandler();

				subscriptionHandler = new SubscriptionHandler(
					subscriptionWrapChannel,
					hub,
					commandHandlers,
					config,
					logger);

				subscriptionHandler.AddHandler(handlersConfig);

				topicListenerConsumerEventHandlers.AddReceiveHandler(subscriptionHandler.Handle);

			} catch (Exception e) {
					throw new InvalidOperationException(
						$"Cant create {nameof(SubscriptionHandler)} " +
						$"Error: {e.Message} ", e);

			}
			return subscriptionHandler;
		}
	}
}
