using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib {
	internal class ResponseConsumerBinder : IConsumerBinder {
		private readonly IModel channel;

		public ResponseConsumerBinder(IModel channel) {
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
