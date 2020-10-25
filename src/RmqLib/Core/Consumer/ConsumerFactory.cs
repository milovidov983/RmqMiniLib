using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib {
	internal class ConsumerFactory : IConsumerFactory {
		private readonly IModel channel;
		private readonly IConsumerBinder consumerBinder;

		public ConsumerFactory(IModel channel, IConsumerBinder consumerBinder) {
			this.channel = channel;
			this.consumerBinder = consumerBinder;
		}

		public AsyncEventingBasicConsumer CreateBasicConsumer() {
			var consumerInstance = new AsyncEventingBasicConsumer(channel);
			consumerBinder.Bind(consumerInstance, channel);
			return consumerInstance;
		}
	}
}
