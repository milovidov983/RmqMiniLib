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
		private readonly Initializer initializer;

		public RabbitHub(RmqConfig config) {
			this.config = config;
			this.initializer = new Initializer(config);
			publisherFactory = initializer.InitPublisherFactory();
		}


		public SubscriptionChannel CreateSubscriptions() {
			return initializer.InitSubscriptions();
		}

		public async Task<TResponse> ExecuteRpcAsync<TResponse, TRequest>(string topic, TRequest request, TimeSpan? timeout = null) where TResponse : class {
			byte[] body = ToByteArray(request);
			var correlationId = Guid.NewGuid().ToString("N");
			var di = new DeliveryInfo(topic, body, correlationId, ServiceConstants.REPLY_QUEUE_NAME);

			var pub = publisherFactory.GetBasicPublisher();
			var dm = pub.CreateRpcPublication(di, timeout);

			await dm.WaitResult();
			if (dm.HasError) {
				throw new Exception(dm.GetError());
			}
			return dm.GetResponse<TResponse>();
		}

		// todo refact
		private byte[] ToByteArray<TRequest>(TRequest request) {
			var json = JsonSerializer.Serialize(request);
			return Encoding.UTF8.GetBytes(json);
		}


		public async Task PublishAsync<TRequest>(string topic, TRequest request, TimeSpan? timeout = null) {
			byte[] body = ToByteArray(request);
			var di = new DeliveryInfo(topic, body, null, null);
			var pub = publisherFactory.GetBasicPublisher();
			await pub.CreateNotify(di, timeout);
		}


		//public void SubscribeAsync(string queueName, Func<DeliveredMessage, Task<MessageProcessResult>> onMessage, int prefetchCount = 32) {
		//	IPublisher inputChannel = channelPool.GetInputChannel(queueName, prefetchCount);
		//	inputChannel.OnMessage = onMessage;

		//}
	}
}
