using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;
using System.Threading.Tasks;

namespace RmqLib {
	internal interface IConnectionManager {
		IConnectionWrapper GetConnection();
		IResponseMessageHandler GetResponseMessageHandler();
		IChannelPool GetRpcChannelPool();
		IChannelPool CreateChannelPool();
	}
}
