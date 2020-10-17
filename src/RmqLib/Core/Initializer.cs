using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal class Initializer {
		private RmqConfig config;
		private ConnectionWrapper connection;

		public Initializer(RmqConfig config) {
			this.config = config;

			DeliveryInfo.AppId = config.AppId;
			DeliveryInfo.ExhangeName = config.Exchange;
		}

		public IConnectionManager InitConnectionManager() {
			var channelEventsHandlerFactory = ChannelEventsHandlerFactory.Create();
			var channelPoolFactory = ChannelPoolFactory.Create(channelEventsHandlerFactory);
			var responseMessageHandlerFactory = ResponseMessageHandlerFactory.Create();

			return new ConnectionManager(
				config, 
				channelPoolFactory, 
				responseMessageHandlerFactory);

		}

		public IPublisherFactory InitPublisherFactory() {

			IConnectionManager connectionManager = InitConnectionManager();



			IResponseMessageHandler replyHandler = new ResponseMessageHandler();
			ConsumerManager consumerInitializer = new ConsumerManager(channel, replyHandler, connectionManager);
			consumerInitializer.InitConsumer();

			return new PublisherFactory(connectionManager, replyHandler);
		}

		public SubscriptionChannel InitSubscriptions() {
			IModel channel = connection.CreateChannel();
			return new SubscriptionChannel(channel);
		}
	}
}
