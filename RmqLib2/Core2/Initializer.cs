using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib2.Core2 {
	public class Initializer {
		private RmqConfig config;

		public void Init() {
			IConnectionWrapper connection = new ConnectionWrapper(config);
			connection.StartConnection();

			IModel channel = connection.CreateChannel();
			IChannelPool channelPool = new ChannelPool(channel);
			IConnectionManager connectionManager = new ConnectionManager(channelPool.GetChannel());

			IReplyHandler replyHandler = new ReplyHandelr();
			ConsumerInitializer consumerInitializer = new ConsumerInitializer(channel, replyHandler, connectionManager);
			consumerInitializer.InitConsumer();

			IPublisherFactory publisherFactory = new PublisherFactory(channelPool);

			//var publisher 
		}
	}
}
