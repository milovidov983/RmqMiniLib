using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib2 {



	internal class BasicPublisher : IPublisher, IDisposable {
		class DeliveryItem {
			public DeliveryItem(DeliveryInfo deliveryInfo, Action<Exception> errorAction, Action successAction = null) {
				ErrorAction = errorAction;
				SuccessAction = successAction;
				DeliveryInfo = deliveryInfo;
			}

			public Action<Exception> ErrorAction { get; }
			public Action SuccessAction { get; }
			public DeliveryInfo DeliveryInfo { get; }

		}



		private readonly IChannelWrapper channel;
		private readonly IReplyHandler replyHandler;
		private readonly BlockingCollection<DeliveryItem> deliveryItems = new BlockingCollection<DeliveryItem>();


		public BasicPublisher(IChannelWrapper channel, IReplyHandler replyHandler) {
			this.channel = channel;
			this.replyHandler = replyHandler;

			Task.Run(async () => await RequestHandlerStartMainLoop());
		}
	
		/// <summary>
		/// Запускаем прослушивание коллекции в которую попадают данные на отправку в рамках RPC вызова
		/// </summary>
		/// <returns></returns>
		private async Task RequestHandlerStartMainLoop() {
			while (!deliveryItems.IsCompleted) {
				var item = deliveryItems.Take();
				var status = await channel.BasicPublish(item.DeliveryInfo);

				if (status.IsSuccess) {
					item.SuccessAction?.Invoke();

				} else {
					// debug
					Console.WriteLine($"{nameof(BasicPublisher)}{nameof(RequestHandlerStartMainLoop)} {item.DeliveryInfo.Topic}" +
						$"{status.Error}");

					item.ErrorAction.Invoke(status.Error);
				}

			}
		}

		public ResponseMessage CreateRpcPublication(DeliveryInfo deliveryInfo, TimeSpan? timeout = null) {
			if (!deliveryItems.IsCompleted) {
				var deliveryMessage = CreateDeliveryMessage(deliveryInfo, timeout);

				var deliveryItem = new DeliveryItem(deliveryInfo, deliveryMessage.ResponseTask.SetException);
				deliveryItems.Add(deliveryItem);

				return deliveryMessage;
			}
			throw new InvalidOperationException("Очередь запросов завершила свою работу requests.IsCompleted == true");
		}

		public Task CreateNotify(DeliveryInfo deliveryInfo, TimeSpan? timeout = null) {
			if (!deliveryItems.IsCompleted) {
				var tsc = new TaskCompletionSource<bool>();

				var timer = CreateTimer(timeout, () => {
					tsc.SetCanceled();
				});

				var deliveryItem = new DeliveryItem(
					deliveryInfo, 
					errorAction: tsc.SetException, 
					successAction: () => {
						timer.Enabled = false;
						tsc.SetResult(true);
					});
				deliveryItems.Add(deliveryItem);

				return tsc.Task;
			}
			throw new InvalidOperationException("Очередь запросов завершила свою работу requests.IsCompleted == true");
		}



		private ResponseMessage CreateDeliveryMessage(DeliveryInfo deliveryInfo, TimeSpan? timeout) {
			var timer = CreateTimer(timeout, () => {
				var responseMessage = replyHandler.RemoveReplySubscription(deliveryInfo.CorrelationId);
				responseMessage.SetElapsedTimeout();
			});

			var task = new ResponseTask(timer);
			var resp = new ResponseMessage(task, deliveryInfo.CorrelationId);

			replyHandler.AddReplySubscription(deliveryInfo.CorrelationId, resp);

			return resp;
		}

		private const int DEFAULT_TIMEOUT_MS = 5;

		private System.Timers.Timer CreateTimer(TimeSpan? timeout, Action timeoutAction) {
			timeout = timeout ?? new TimeSpan(0, 0, DEFAULT_TIMEOUT_MS);

			var timer = new System.Timers.Timer(timeout.Value.TotalMilliseconds) {
				Enabled = true
			};

			timer.Elapsed += (object source, ElapsedEventArgs e) => {
				timer.Enabled = false;
				timeoutAction.Invoke();
			};

			return timer;
		}
		public void Dispose() {
			deliveryItems.CompleteAdding();
			channel.Close();
		}
	}
}
