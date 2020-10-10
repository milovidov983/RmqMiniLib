using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib2 {


	public class RabbitHub : IRabbitHub {
		private IPublisherFactory publisherFactory;
		private readonly RmqConfig config;

		public RabbitHub(RmqConfig config) {
			this.config = config;
			var init = new Initializer(config);
			publisherFactory = init.InitPublisherFactory();
		}



		public async Task<TResponse> ExecuteRpcAsync<TResponse, TRequest>(string topic, TRequest request, TimeSpan? timeout = null) where TResponse : class {
			byte[] body = ToByteArray(request);
			var correlationId = Guid.NewGuid().ToString("N");
			var di = new DeliveryInfo(config.Exchange, topic, body, correlationId);
			var dm = ExecuteRpcAsync(di, timeout);
			
			await dm.WaitResult();
			if (dm.HasError) {
				throw new Exception(dm.GetError());
			}
			var resp = dm.GetResponse<TResponse>();

			return resp;
		}

		private byte[] ToByteArray<TRequest>(TRequest request) {
			var json = JsonSerializer.Serialize(request);
			return Encoding.UTF8.GetBytes(json);
		}

		private DeliveredMessage ExecuteRpcAsync(DeliveryInfo deliveryInfo, TimeSpan? timeout = null) {
			var pub = publisherFactory.GetBasicPublisher();

			var dm = pub.Publish(deliveryInfo, timeout);

			return dm;
		}




		


		//public Task PublishAsync(DeliveryInfo deliveryInfo, Payload payload, CancellationToken token) {
		//	throw new NotImplementedException();
		//}

		//public void SubscribeAsync(string queueName, Func<DeliveredMessage, Task<MessageProcessResult>> onMessage, int prefetchCount = 32) {
		//	IPublisher inputChannel = channelPool.GetInputChannel(queueName, prefetchCount);
		//	inputChannel.OnMessage = onMessage;

		//}
	}
}
