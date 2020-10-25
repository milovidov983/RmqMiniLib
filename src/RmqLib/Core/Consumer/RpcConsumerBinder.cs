using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib {
	internal class RpcConsumerBinder : IConsumerBinder {

		/// <summary>
		/// Привязка канала обслуживающего RPC
		/// </summary>
		public void Bind(AsyncEventingBasicConsumer consumerInstance, IModel channel) {
			channel.BasicConsume(
				consumer: consumerInstance,
				// специальная встроенная в RabbitMQ очередь для получения ответов на RPC запросы
				queue: ServiceConstants.REPLY_QUEUE_NAME,  
				autoAck: true);
		}
	}
}
