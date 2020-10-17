using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib {
	internal class ConsumerFactory : IConsumerFactory {
		private readonly IModel channel;

		public ConsumerFactory(IModel channel) {
			this.channel = channel;
		}

		public AsyncEventingBasicConsumer CreateBasicConsumer() {
			return new AsyncEventingBasicConsumer(channel);
		}
	}
}
