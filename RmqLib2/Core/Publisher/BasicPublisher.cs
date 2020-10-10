﻿using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib2 {
	internal class BasicPublisher : IPublisher, IDisposable {
		private readonly IChannelWrapper channel;
		private readonly IReplyHandler replyHandler;
		private readonly BlockingCollection<DeliveryInfo> requests = new BlockingCollection<DeliveryInfo>();


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
			while (!requests.IsCompleted) {
				var deliveryInfo = requests.Take();
				var publishStatus = await channel.BasicPublish(deliveryInfo);

				if (!publishStatus.IsSuccess) {
					Console.WriteLine($"{nameof(BasicPublisher)}{nameof(RequestHandlerStartMainLoop)} {publishStatus.Error}");

					requests.TryAdd(deliveryInfo);
				}

			}
		}



		public DeliveredMessage Publish(DeliveryInfo deliveryInfo, TimeSpan? timeout = null) {
			if (!requests.IsCompleted) {
				var deliveryMessage = CreateDeliveryMessage(deliveryInfo, timeout);

				requests.Add(deliveryInfo);

				return deliveryMessage;
			}
			throw new InvalidOperationException("Очередь запросов завершила свою работу requests.IsCompleted == true");
		}

		private DeliveredMessage CreateDeliveryMessage(DeliveryInfo deliveryInfo, TimeSpan? timeout) {
			var timer = CreateTimer(timeout, deliveryInfo.CorrelationId);
			var task = new ResponseTask(timer);
			var dm = new DeliveredMessage(task, deliveryInfo.CorrelationId);
			replyHandler.AddReplySubscription(deliveryInfo.CorrelationId, dm);
			return dm;
		}

		private System.Timers.Timer CreateTimer(TimeSpan? timeout, string correlationId) {
			timeout = timeout ?? new TimeSpan(0, 0, 5);
			var timer = new System.Timers.Timer(timeout.Value.TotalMilliseconds) {
				Enabled = true
			};

			timer.Elapsed += (object source, ElapsedEventArgs e) => {
				var dm = replyHandler.RemoveReplySubscription(correlationId);
				//dm.SetElapsedTimeout(timeout.Value.TotalMilliseconds);
				timer.Enabled = false;
			};

			return timer;
		}
		public void Dispose() {
			requests.CompleteAdding();
			channel.Close();
		}
	}
}
