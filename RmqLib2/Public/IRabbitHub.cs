using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib2 {
	public interface IRabbitHub {
		Task PublishAsync(DeliveryInfo deliveryInfo, Payload payload, CancellationToken token);
		Task<DeliveredMessage> ExecuteRpcAsync(DeliveryInfo deliveryInfo,  Payload payload, CancellationToken token);
		void SubscribeAsync(string queueName, Func<DeliveredMessage, Task<MessageProcessResult>> onMessage, int prefetchCount = 32);

	}
}
