using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {

	internal class PublisherFactory : IPublisherFactory {
		private readonly IPublisher basicPublisher;

		public PublisherFactory(IChannelPool channelPool, IReplyHandler replyHandler) {
			basicPublisher = new BasicPublisher(channelPool.GetChannel(), replyHandler);
		}

		/// <summary>
		/// Получить объект умеющий публиковать сообщения в rmq
		/// </summary>
		/// <returns></returns>
		public IPublisher GetBasicPublisher() {
			
			return basicPublisher;
		}

		
	}
}