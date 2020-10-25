using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RmqLib.Core {
	internal interface IConsumerBinder {
		void Bind(IBasicConsumer consumerInstance, IChannelWrapper channel);
	}
}
