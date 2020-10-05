using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal interface IChannelFactory {
		IChannelWrapper GetChannel();
		Task InitChannel(IModel channel);
	}
}