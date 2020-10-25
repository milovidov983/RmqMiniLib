﻿using RabbitMQ.Client;
using RmqLib.Core;
using RmqLib.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib {
	public class RabbitHub : IRabbitHub {
		private readonly IPublisherFactory publisherFactory;
		private readonly Initializer initializer;
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
		private readonly RmqConfig config;
		private IModel subscriptionChannel;


		public RabbitHub(RmqConfig config) {
			this.initializer = new Initializer(config);
			initializer.InitConnectionManager();
			publisherFactory = initializer.InitPublisherFactory();
			this.config = config;
		}


		public SubscriptionManager CreateSubscriptions(Dictionary<string, IRabbitCommand> commandHandlers) {
			var connectionManager = initializer.connectionManager;
			connectionManager.CreateSubscriptionChannelPool(config.PrefetchCount);
			subscriptionChannel = connectionManager.GetSubscriptionChannel();

			return initializer.InitSubscriptions(this, commandHandlers);
		}

		public async Task<TResponse> ExecuteRpcAsync<TResponse, TRequest>(string topic, TRequest request, TimeSpan? timeout = null) where TResponse : class {
			byte[] body = request.ToByteArray();
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

		public async Task PublishAsync<TRequest>(string topic, TRequest request, TimeSpan? timeout = null) {
			byte[] body = request.ToByteArray();
			var di = new DeliveryInfo(topic, body, null, null);
			var pub = publisherFactory.GetBasicPublisher();
			await pub.CreateBroadcastPublication(di, timeout);
		}

		public async Task SetRpcErrorAsync(DeliveredMessage dm, string error, int? statusCode = null) {
			if (!dm.IsRpcMessage()) {
				throw new InvalidOperationException("Can't reply on non-RPC request");
			}

			try {
				await semaphore.WaitAsync();
				var replyProps = subscriptionChannel.CreateBasicProperties();
				replyProps.CorrelationId = dm.ReplyProps.CorrelationId;

				replyProps.Headers = replyProps.Headers ?? new Dictionary<string, object>();
				replyProps.Headers.Add(RmqLib.Core.Headers.Error, error);
				replyProps.Headers.Add(RmqLib.Core.Headers.StatusCode, statusCode);

				subscriptionChannel.BasicPublish(
					exchange: "",
					routingKey: dm.ReplyProps.ReplyTo,
					basicProperties: replyProps
					);

			} finally {
				semaphore.Release();
			}
		}

		public async Task SetRpcResultAsync<T>(DeliveredMessage dm, T payload, int? statusCode = null) {
			if (!dm.IsRpcMessage()) {
				throw new InvalidOperationException("Can't reply on non-RPC request");
			}

			var resp = JsonSerializer.Serialize(payload);
			byte[] respBody = Encoding.UTF8.GetBytes(resp);
			try {
				await semaphore.WaitAsync();
				var replyProps = subscriptionChannel.CreateBasicProperties();
				replyProps.CorrelationId = dm.ReplyProps.CorrelationId;
				
				subscriptionChannel.BasicPublish(
					exchange: "", 
					routingKey: dm.ReplyProps.ReplyTo, 
					basicProperties: replyProps, 
					body: respBody);

			} finally {
				semaphore.Release();
			}
		}

	}
}
