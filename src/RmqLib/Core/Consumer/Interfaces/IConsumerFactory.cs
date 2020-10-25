using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib {
	internal interface IConsumerFactory {
		IAsyncBasicConsumer CreateBasicConsumer();
	}
}
