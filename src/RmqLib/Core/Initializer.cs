using RabbitMQ.Client;
using RmqLib.Core.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal class Initializer {
		private RmqConfig config;
		private ConnectionWrapper connection;
		private IRmqLogger logger;

		public Initializer(RmqConfig config) {
			this.config = config;

			DeliveryInfo.AppId = config.AppId;
			DeliveryInfo.ExhangeName = config.Exchange;

			var loggerFactory = LoggerFactory.Create();
			logger = loggerFactory.CreateLogger();
		}

		public IConnectionManager InitConnectionManager() {
			var channelEventsHandlerFactory = ChannelEventsHandlerFactory.Create(logger);
			var channelPoolFactory = ChannelPoolFactory.Create(channelEventsHandlerFactory);
			var responseMessageHandlerFactory = ResponseMessageHandlerFactory.Create();

			return new ConnectionManager(
				config, 
				channelPoolFactory, 
				responseMessageHandlerFactory,
				logger);

		}

		public IPublisherFactory InitPublisherFactory() {
			IConnectionManager connectionManager = InitConnectionManager();
			IResponseMessageHandler replyHandler = new ResponseMessageHandler();
		

			return new PublisherFactory(connectionManager.GetRpcChannelPool(), replyHandler);
		}

		public SubscriptionChannel InitSubscriptions() {
			IModel channel;
			try {
				channel = connection.CreateChannel();
			} catch (Exception e) {
					throw new ArgumentNullException(
						$"Cant create {nameof(SubscriptionChannel)} be cause " +
						$"{nameof(connection)} could not create {nameof(channel)}." +
						$"Error: {e.Message} ", e);

			}
			return new SubscriptionChannel(channel);
		}
	}
}
