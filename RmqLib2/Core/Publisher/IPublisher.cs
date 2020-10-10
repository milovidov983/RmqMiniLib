using System;

namespace RmqLib2 {
	/// <summary>
	/// Умеет публиковать сообщения в rmq
	/// </summary>
	internal interface IPublisher {
		//Func<DeliveredMessage, Task<MessageProcessResult>> OnMessage { get; set; }

		DeliveredMessage Publish(DeliveryInfo deliveryMessage, TimeSpan? timeout = null);
	}
}
