using RabbitMQ.Client;

namespace RmqLib.Core {
	internal interface IChannelEventsHandlerFactory {
		IChannelEventsHandler CreateHandler();
	}
}