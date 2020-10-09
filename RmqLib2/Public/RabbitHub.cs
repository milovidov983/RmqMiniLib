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
		public RabbitHub(string connectionString) {
			// create connectionManager будет создавать соединение и следить
			// за его состоянием при необходимости переподключатся и создавать каналы
			var cm = new ConnectionWrapperFactory(connectionString);
			channelPool = cm.CreatechannelPool();


		}


		internal void SetQueueHandlersConfig(IQueueHandlersConfig queueConfig) {
			this.queueConfig = queueConfig;
		}

		public Task<DeliveredMessage> ExecuteRpcAsync(DeliveryInfo deliveryInfo, Payload payload, TimeSpan? timeout = null) {
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
