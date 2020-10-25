using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib {
	internal class TopicListinerConsumerBinder : IConsumerBinder {
		private readonly string queueName;

		public TopicListinerConsumerBinder(string queueName) {
			this.queueName = queueName;
		}

		/// <summary>
		/// Привязка канала обслуживающего подписки на топики
		/// </summary>
		public void Bind(AsyncEventingBasicConsumer consumerInstance, IModel channel) {
			channel.BasicConsume(
				consumer: consumerInstance,
				queue: queueName,
				autoAck: false);
		}
	}
}
