using System;

namespace RmqLib2 {
	internal interface IPublisher {
		//Func<DeliveredMessage, Task<MessageProcessResult>> OnMessage { get; set; }

		DeliveredMessage Publish(DeliveryInfo deliveryMessage, TimeSpan? timeout = null);
	}
}
