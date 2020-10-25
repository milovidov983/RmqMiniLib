using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib.Core {

	internal class PublisherFactory : IPublisherFactory {
		private readonly IPublisher basicPublisher;

		public PublisherFactory(IConnectionManager connectionManager, RmqConfig rmqConfig, IRmqLogger logger) {

			IChannelPool channelPool = connectionManager.GetRpcChannelPool();
			IResponseMessageHandler replyHandler = connectionManager.GetResponseMessageHandler();
			
			basicPublisher = new BasicPublisher(channelPool.GetChannelWrapper(), replyHandler, rmqConfig, logger);
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