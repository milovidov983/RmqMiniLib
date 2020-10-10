using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {

	internal class PublisherFactory : IPublisherFactory {
		private readonly IPublisher basicPublisher;

		public PublisherFactory(IChannelPool channelPool, IReplyHandler replyHandler) {
			basicPublisher = new BasicPublisher(channelPool.GetChannel(), replyHandler);
		}

		public IPublisher GetBasicPublisher() {
			
			return basicPublisher;
		}

		
	}
}