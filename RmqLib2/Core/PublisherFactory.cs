using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class PublisherFactory : IPublisherFactory {
		private ConnectionManager connectionManager;

		private IPublisher basicPublisher;

		private readonly string exchangeName;

		public PublisherFactory(ConnectionManager connectionManager, string exchangeName) {
			this.connectionManager = connectionManager;
			this.exchangeName = exchangeName;

			var channel = CreateChanel();

			basicPublisher = new BasicPublisher(connectionManager, channel);
		}

		public IPublisher GetBasicPublisher() {
			return basicPublisher;
		}

		public async Task<IModel> CreateChanel() {
			var factory = await connectionManager.CreateChannelFactory();
			if(factory.Status == ChannelCreatedStatus.Success) {
				return CreateChanel(factory.Create());

			}
			throw new CreateChannelException(factory.Details);
		}

		private IModel CreateChanel(IModel channel) {
			DeclareExchanges(channel);
			return channel;
		}

		private void DeclareExchanges(IModel channel) {
			channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);
		}
	}
}