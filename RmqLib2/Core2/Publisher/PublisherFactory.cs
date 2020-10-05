using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {

	internal class PublisherFactory : IPublisherFactory {
		private IPublisher basicPublisher;

		public PublisherFactory(IReconnectionManager reconnectionManager, IChannelFactory channelFactory) {
			basicPublisher = new BasicPublisher(reconnectionManager, channelFactory);
		}

		public IPublisher GetBasicPublisher() {
			
			return basicPublisher;
		}

		
	}
}