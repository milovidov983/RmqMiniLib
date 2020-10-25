using RabbitMQ.Client;
using RmqLib.Core.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal class Initializer {
		private RmqConfig config;
		private IRmqLogger logger;
		private IConnectionManager connectionManager;

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

		public SubscriptionChannel InitSubscriptions() {
			SubscriptionChannel subscription;
			try {
				var pool = connectionManager.GetSubsChannelPool();
				var channelWrapper = pool.GetChannel();
				var channel = channelWrapper.GetDebugChannel();
				subscription = new SubscriptionChannel(channel);

			} catch (Exception e) {
					throw new ArgumentNullException(
						$"Cant create {nameof(SubscriptionChannel)} " +
						$"Error: {e.Message} ", e);

			}
			return subscription;
		}
	}
}
