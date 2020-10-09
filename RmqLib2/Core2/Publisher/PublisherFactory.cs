using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {

	internal class PublisherFactory : IPublisherFactory {
		private IPublisher basicPublisher;

		public PublisherFactory(IReconnectionManager reconnectionManager, IChannelPool channelPool) {
			basicPublisher = new BasicPublisher(reconnectionManager, channelPool);
		}

		public IPublisher GetBasicPublisher() {
			
			return basicPublisher;
		}

		
	}
}