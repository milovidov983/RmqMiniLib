using RabbitMQ.Client;

namespace RmqLib.Core {
	internal interface IChannelPoolFactory {
		IChannelPool CreateChannelPool(IModel model);
	}
}