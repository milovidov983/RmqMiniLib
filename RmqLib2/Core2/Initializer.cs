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
			IReplyHandler replyHandler = new ReplyHandelr();
			IChannelPool channelPool = new ChannelPool(channel, replyHandler);
			IReconnectionManager reconnectionManager = new RecconectionManager(connection, channelPool);
			IPublisherFactory publisherFactory = new PublisherFactory(reconnectionManager, channelPool);

			//var publisher 
		}
	}
}
