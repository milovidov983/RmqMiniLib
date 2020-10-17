using RabbitMQ.Client.Events;

namespace RmqLib {
	internal interface IConsumerFactory {
		AsyncEventingBasicConsumer CreateBasicConsumer();
	}
}
