using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib2 {
	internal class BasicPublisher : IPublisher, IDisposable {
		private readonly IChannelPool channelPool;
		private readonly IReplyHandler replyHandler;
		private readonly BlockingCollection<DeliveryInfo> requests = new BlockingCollection<DeliveryInfo>();


		public BasicPublisher(IChannelPool channelPool, IReplyHandler replyHandler) {
			this.channelPool = channelPool;
			this.replyHandler = replyHandler;

			Task.Run(async () => await RequestHandlerStartMainLoop());
		}

		/// <summary>
		/// Запускаем прослушивание коллекции в которую попадают данные на отправку в рамках RPC вызова
		/// </summary>
		/// <returns></returns>
		private async Task RequestHandlerStartMainLoop() {
			while (!requests.IsCompleted) {
				var deliveryInfo = requests.Take();
				var channel = channelPool.GetChannel();
				var publishStatus = await channel.BasicPublish(deliveryInfo);

				if (!publishStatus.IsSuccess) {
					Console.WriteLine($"{nameof(BasicPublisher)}{nameof(RequestHandlerStartMainLoop)} {publishStatus.Error}");

					requests.TryAdd(deliveryInfo);
				}
				
			}
		}



		public DeliveredMessage Publish(DeliveryInfo deliveryInfo, TimeSpan? timeout = null) {
			if (!requests.IsCompleted) {
				requests.Add(deliveryInfo);
			}
			var timer = CreateTimer(timeout, deliveryInfo.CorrelationId);
			var task = new ResponseTask(timer);
			var dm = new DeliveredMessage(task, deliveryInfo.CorrelationId);
			timer.Start();
			return dm;
		}
		private System.Timers.Timer CreateTimer(TimeSpan? timeout, string correlationId) {
			timeout = timeout ?? new TimeSpan(0, 0, 20);
			var timer = new System.Timers.Timer(timeout.Value.TotalMilliseconds) {
				Enabled = true
			};

			timer.Elapsed += (object source, ElapsedEventArgs e) => {
				var dm = replyHandler.RemoveReplySubscription(correlationId);
				dm.SetElapsedTimeout(timeout.Value.TotalMilliseconds);
			};

			return timer;
		}
		public void Dispose() {
			requests.CompleteAdding();
			var channel = channelPool.GetChannel();
			channel.Close();
		}
	}
}
