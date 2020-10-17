using RabbitMQ.Client;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal interface IChannelPool {
		IChannelWrapper GetChannel();
	}
}