using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib {
	internal class ConsumerBinder : IConsumerBinder {
		private readonly IModel channel;

		public ConsumerBinder(IModel channel) {
			this.channel = channel;
		}

		public void Bind(AsyncEventingBasicConsumer consumerInstance) {
			channel.BasicConsume(
				consumer: consumerInstance,
				queue: ServiceConstants.REPLY_QUEUE_NAME,
				autoAck: true);
		}
	}
}
