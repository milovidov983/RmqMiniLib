using System;
using System.Threading.Tasks;

namespace RmqLib.Core {
	/// <summary>
	/// Умеет публиковать сообщения в rmq
	/// </summary>
	internal interface IPublisher {
		//Func<DeliveredMessage, Task<MessageProcessResult>> OnMessage { get; set; }
		Task CreateBroadcastPublication(DeliveryInfo deliveryInfo, TimeSpan? timeout = null);
		ResponseMessage CreateRpcPublication(DeliveryInfo deliveryMessage, TimeSpan? timeout = null);
	}
}
