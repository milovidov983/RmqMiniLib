using RabbitMQ.Client;

namespace RmqLib {
	public interface IChannelFactory {
		IModel CreateChannel(int prefechCount);
	}
}