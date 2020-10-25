using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib {
	public interface IConsumerBinder {
		void Bind(AsyncEventingBasicConsumer consumerInstance, IModel channel);
	}
}
