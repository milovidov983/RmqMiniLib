﻿using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {

	internal class PublisherFactory : IPublisherFactory {
		private IPublisher basicPublisher;

		public PublisherFactory(IChannelPool channelPool) {
			basicPublisher = new BasicPublisher(channelPool);
		}

		public IPublisher GetBasicPublisher() {
			
			return basicPublisher;
		}

		
	}
}