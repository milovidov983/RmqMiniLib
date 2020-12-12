using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RmqLib.Core;
using RmqLib.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RmqLib {
	public class RabbitHub : IRabbitHub {
		public RmqConfig Config { get; }

		private readonly IPublisherFactory publisherFactory;
		private readonly Initializer initializer;
		private IChannelWrapper subscriptionChannel;
		private IConnectionManager connectionManager;

		public RabbitHub(RmqConfig config, ILogger externalLogger = null) {
			this.initializer = new Initializer(config, externalLogger);
			initializer.InitConnectionManager();
			publisherFactory = initializer.InitPublisherFactory();
			this.Config = config;
		}


		internal SubscriptionManager CreateSubscriptions(Dictionary<string, IRabbitCommand> commandHandlers) {
			this.connectionManager = initializer.connectionManager;
			connectionManager.CreateSubscriptionChannelPool(Config.PrefetchCount);
			subscriptionChannel = connectionManager.GetSubscriptionChannel();

			return initializer.InitSubscriptions(this, commandHandlers);
		}

		public async Task<TResponse> ExecuteRpcAsync<TResponse, TRequest>(string topic, TRequest request, TimeSpan? timeout = null)
			where TResponse : class
			where TRequest : class {

			byte[] body = request.ToByteArray();
			var correlationId = Guid.NewGuid().ToString("N");
			var di = new DeliveryInfo(topic, body, correlationId, ServiceConstants.REPLY_QUEUE_NAME);

			IPublisher pub = publisherFactory.GetBasicPublisher();
			ResponseMessage rm = pub.CreateRpcPublication(di, timeout);

			await rm.WaitResult();
			if (rm.HasError) {
				throw new Exception(rm.GetError());
			}
			return rm.GetResponse<TResponse>();
		}

		public async Task PublishAsync<TRequest>(string topic, TRequest request, TimeSpan? timeout = null) where TRequest : class {
			byte[] body = request.ToByteArray();
			var di = new DeliveryInfo(topic, body, null, null);
			IPublisher pub = publisherFactory.GetBasicPublisher();
			await pub.CreateBroadcastPublication(di, timeout);
		}

		public async Task SetRpcErrorAsync(DeliveredMessage dm, string error, int? statusCode = null) {
			if (!dm.IsRpcMessage()) {
				throw new InvalidOperationException("Can't reply on non-RPC request");
			}


			IBasicProperties replyProps = await subscriptionChannel.CreateBasicProperties();
			replyProps.CorrelationId = dm.ReplyProps.CorrelationId;

			replyProps.Headers = replyProps.Headers ?? new Dictionary<string, object>();
			replyProps.Headers.Add(Core.Headers.Error, error);
			replyProps.Headers.Add(Core.Headers.StatusCode, statusCode);

			await subscriptionChannel.BasicPublish(
				exchange: "",
				routingKey: dm.ReplyProps.ReplyTo,
				basicProperties: replyProps
				);

		}

		public async Task SetRpcResultAsync<T>(DeliveredMessage dm, T payload, int? statusCode = null) {
			if (!dm.IsRpcMessage()) {
				throw new InvalidOperationException("Can't reply on non-RPC request");
			}

			var resp = JsonSerializer.Serialize(payload, JsonOptions.Default);
			byte[] respBody = Encoding.UTF8.GetBytes(resp);

			IBasicProperties replyProps = await subscriptionChannel.CreateBasicProperties();
			replyProps.CorrelationId = dm.ReplyProps.CorrelationId;

			await subscriptionChannel.BasicPublish(
				exchange: "",
				routingKey: dm.ReplyProps.ReplyTo,
				basicProperties: replyProps,
				body: respBody);


		}


		public void Close() {
			var channelOne = connectionManager.GetRpcChannel();
			channelOne.Close();
			var channelTwo = connectionManager.GetSubscriptionChannel();
			channelTwo.Close();
			var conn = connectionManager.GetConnection();
			conn.Abort();
		}

	}
}
