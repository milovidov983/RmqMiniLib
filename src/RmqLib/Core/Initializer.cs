using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib2 {
	internal class Initializer {
		private RmqConfig config;
		private ConnectionWrapper connection;

		public Initializer(RmqConfig config) {
			this.config = config;

			DeliveryInfo.AppId = config.AppId;
			DeliveryInfo.ExhangeName = config.Exchange;
		}

		public IPublisherFactory InitPublisherFactory() {
			connection = new ConnectionWrapper(config);

			IModel channel = connection.CreateChannel();
			IChannelPool channelPool = new ChannelPool(channel);
			IConnectionManager connectionManager = new ConnectionManager(channelPool.GetChannel());
			connection.AddConnectionShutdownHandler(connectionManager);

			IReplyHandler replyHandler = new ReplyHandelr();
			ConsumerInitializer consumerInitializer = new ConsumerInitializer(channel, replyHandler, connectionManager);
			consumerInitializer.InitConsumer();

			return new PublisherFactory(channelPool, replyHandler);
		}

		public SubscriptionChannel InitSubscriptions() {
			IModel channel = connection.CreateChannel();
			return new SubscriptionChannel(channel);
		}
	}
}
