using RabbitMQ.Client;
using RmqLib.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RmqLib.Core {
	internal class Initializer {
		private RmqConfig config;
		private IRmqLogger logger;
		public IConnectionManager connectionManager;

		public Initializer(RmqConfig config) {
			this.config = config;

			DeliveryInfo.AppId = config.AppId;
			DeliveryInfo.ExhangeName = config.Exchange;

			var loggerFactory = LoggerFactory.Create();
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

		public SubscriptioManager InitSubscriptions(IRabbitHub hub) {
			SubscriptioManager subscription;
			try {
				//var pool = connectionManager.CreateNewChannelPool();
				//var channelWrapper = pool.GetChannel();
				//var channel = channelWrapper.GetDebugChannel();
				SubscrSettings[] subscriptions = Array.Empty<SubscrSettings>();
				var channelPool = connectionManager.CreateChannelPool();
				var trueChannel = channelPool.GetChannelWrapper().GetTrueChannel();

				IConsumerBinder consumerBinder = new SubscriptionConsumerBinder(trueChannel, config.Queue);
				IConsumerFactory consumerFactory = new ConsumerFactory(trueChannel);

				var consumerManager = new ConsumerManager(consumerFactory, consumerBinder, logger);

				ISubscriptionManager subscriptionManager = new SubscriptioManager(null, subscriptions, hub, config.PrefetchCount);

				IConsumerEventHandlersFactory consumerEventHandlersFactory
					= ConsumerEventHandlersFactory.Create(logger, consumerManager);
				var consumerEventHandlers = consumerEventHandlersFactory.CreateHandler();

				consumerEventHandlers.AddHandler(subscriptionManager.Handler);

			} catch (Exception e) {
					throw new ArgumentNullException(
						$"Cant create {nameof(SubscriptioManager)} " +
						$"Error: {e.Message} ", e);

			}
			return subscription;
		}
	}
}
