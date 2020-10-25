using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib {
	internal class SubscriptionConsumerBinder : IConsumerBinder {
		private readonly IModel channel;
		private readonly string queueName;

		public SubscriptionConsumerBinder(IModel channel, string queueName) {
			this.channel = channel;
			this.queueName = queueName;
		}

		public void Bind(AsyncEventingBasicConsumer consumerInstance) {
			channel.BasicConsume(
				consumer: consumerInstance,
				queue: queueName,
				autoAck: false);
		}
	}
}
