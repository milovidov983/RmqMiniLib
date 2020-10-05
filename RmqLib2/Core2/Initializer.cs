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
			IChannelFactory channelFactory = new ChannelFactory(channel, replyHandler);
			IReconnectionManager reconnectionManager = new RecconectionManager(connection, channelFactory);
			IPublisherFactory publisherFactory = new PublisherFactory(reconnectionManager, channelFactory);

			//var publisher 
		}
	}
}
