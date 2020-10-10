using RmqLib2.Core2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib2 {


	public class RabbitHub : IRabbitHub {
		private IPublisherFactory channelPool;
		private IQueueHandlersConfig queueConfig;
		private IReplyHandler replyHandler;
		private IPublisherFactory publisherFactory;
		private readonly RmqConfig config;

		public RabbitHub(RmqConfig config) {
			var init = new Initializer(config);
			publisherFactory = init.InitPublisherFactory();
			this.config = config;
		}


		internal void SetQueueHandlersConfig(IQueueHandlersConfig queueConfig) {
			this.queueConfig = queueConfig;
		}

		public Task<TResponse> ExecuteRpcAsync<TResponse, TRequest>(string topic, TRequest request, TimeSpan? timeout = null) {
			byte[] body = ToByteArray(request);
			var correlationId = Guid.NewGuid().ToString("N");
			var timer = CreateTimer(timeout, correlationId);
			var task = new ResponseTask(timer);
			var dm = new DeliveredMessage(task, correlationId);
			var di = new DeliveryInfo(config.Exchange, topic, body, dm, timer);

			// Осталось чучуть дожать...

		}

		private byte[] ToByteArray<TRequest>(TRequest request) {
			throw new NotImplementedException();
		}

		public Task<DeliveredMessage> ExecuteRpcAsync(DeliveryInfo deliveryInfo, Payload payload, TimeSpan? timeout = null) {
			var pub = publisherFactory.GetBasicPublisher();

			pub.Publish()
			IPublisher outputChannel = channelPool.CreateChanel();
			
			var timer = CreateTimer(timeout, deliveryInfo.CorrelationId);
			var task = new ResponseTask(timer);
			var dm = new DeliveredMessage(task);
			replyHandler.AddReplySubscription(deliveryInfo.CorrelationId, dm);



			outputChannel.Send(deliveryInfo, payload, task);

			return task.Task;
		}

		private System.Timers.Timer CreateTimer(TimeSpan? timeout, string correlationId) {
			timeout = timeout ?? new TimeSpan(0,0,20);
			var timer = new System.Timers.Timer(timeout.Value.TotalMilliseconds) {
				Enabled = true
			};

			timer.Elapsed += (object source, ElapsedEventArgs e) => {
				var complTask = replyHandler.RemoveReplySubscription(correlationId);
				complTask.ResponseTask.SetException(new OperationCanceledException($"RMQ request timeout after {timeout.Value.TotalSeconds} sec"));
				timer.Dispose();
			};
			return timer;
		}


		private static async Task<TResponse> GetRusult<TResponse>(TaskCompletionSource<byte[]> completionTask, System.Timers.Timer timer)
			where TResponse : class {

			var res = await completionTask.Task;
			timer.Stop();
			var json = System.Text.Encoding.UTF8.GetString(res);
			if (!string.IsNullOrWhiteSpace(json)) {
				return Deserialize<TResponse>(json);
			}
			return default;
		}

		private static TResponse Deserialize<TResponse>(string res) where TResponse : class {
			try {
				return JsonSerializer.Deserialize<TResponse>(res);
			} catch (Exception exteption) {
				var exceptionMessage
					= $"Deserialize to type \"{typeof(TResponse).FullName}\" error: {exteption.Message}";
				throw new RmqException(
					exceptionMessage,
					exteption,
					Error.INTERNAL_ERROR);
			}
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
