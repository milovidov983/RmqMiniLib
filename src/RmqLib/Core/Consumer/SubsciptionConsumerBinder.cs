using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib.Core {
	internal class SubsciptionConsumerBinder : IConsumerBinder {
		private readonly string queueName;

		public SubsciptionConsumerBinder(string queueName) {
			this.queueName = queueName;
		}

		/// <summary>
		/// Привязка канала обслуживающего подписки на топики
		/// </summary>
		public void Bind(IBasicConsumer consumerInstance, IChannelWrapper channel) {
			channel.BasicConsume(
				consumer: consumerInstance,
				queue: queueName,
				autoAck: false);
		}
	}
}
