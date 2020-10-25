using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib {
	public interface IRabbitHub {
		//Task PublishAsync(DeliveryInfo deliveryInfo, Payload payload, CancellationToken token);
		//Task<DeliveredMessage> ExecuteRpcAsync(DeliveryInfo deliveryInfo,  Payload payload, CancellationToken token);
		//void SubscribeAsync(string queueName, Func<DeliveredMessage, Task<MessageProcessResult>> onMessage, int prefetchCount = 32);
		Task PublishAsync<TRequest>(string topic, TRequest request, TimeSpan? timeout = null);
		Task<TResponse> ExecuteRpcAsync<TResponse, TRequest>(string topic, TRequest request, TimeSpan? timeout = null) where TResponse : class;

		SubscriptioManager CreateSubscriptions();

		Task SetRpcErrorAsync(DeliveredMessage dm, string error, int? statusCode = null);
		Task SetRpcResultAsync<T>(DeliveredMessage dm, T payload, int? statusCode = null);


	}
}
