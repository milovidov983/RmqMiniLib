﻿using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib.Core {


	/// <summary>
	/// Класс отвечает за подготовку и отправку rpc и broadcast сообщений
	/// </summary>
	internal partial class BasicPublisher : IPublisher, IDisposable {
		private readonly IChannelWrapper channel;
		private readonly IResponseMessageHandler responseMessageHandler;
		private readonly RmqConfig rmqConfig;
		private readonly BlockingCollection<PublishItem> deliveryItems = new BlockingCollection<PublishItem>();
		private readonly IRmqLogger logger;

		public BasicPublisher(
			IChannelWrapper channel,
			IResponseMessageHandler responseMessageHandler,
			RmqConfig rmqConfig, 
			IRmqLogger logger) {

			this.channel = channel;
			this.responseMessageHandler = responseMessageHandler;
			this.rmqConfig = rmqConfig;
			this.logger = logger;
			Task.Run(async () => await RequestHandlerStartMainLoop());
		}

		/// <summary>
		/// Запускаем прослушивание коллекции в которую попадают данные на отправку в рамках RPC вызова
		/// </summary>
		/// <returns></returns>
		private async Task RequestHandlerStartMainLoop() {
			while (!deliveryItems.IsCompleted) {
				var item = deliveryItems.Take();

				var status = await channel.BasicPublish(item);

				
				if (status.IsSuccess) {
					item.PublishSuccessAction?.Invoke();

				} else {
					// debug
					logger.Debug($"{nameof(BasicPublisher)}{nameof(RequestHandlerStartMainLoop)} {item.DeliveryInfo.Topic}" +
						$"{status.Error}");

					item.PublishErrorAction?.Invoke(status.Error);
				}

			}
		}

		public ResponseMessage CreateRpcPublication(DeliveryInfo deliveryInfo, TimeSpan? timeout = null) {
			if (!deliveryItems.IsCompleted) {
				var timer = CreateTimer(timeout);

				var responseMessage = CreateResponse(deliveryInfo, timer);

				// Специальный объект обёртка публикуемого сообщения 
				var publishItem = new PublishItem(
					deliveryInfo,
					errorAction: (e) => { 
						responseMessage.ResponseTask.SetException(e);
						responseMessageHandler.RemoveSubscription(deliveryInfo.CorrelationId);
					});

				timer.Elapsed += (object source, ElapsedEventArgs e) => {
					timer.Enabled = false;
					var rm = responseMessageHandler.RemoveSubscription(deliveryInfo.CorrelationId);
					rm?.SetElapsedTimeout();
					publishItem?.Abort();
				};

				// Подготовленный объект перед отправкой ставится в очередь в коллекцию deliveryItems
				Task.Factory.StartNew(() => deliveryItems.Add(publishItem));

				return responseMessage;
			}
			throw new InvalidOperationException("Очередь запросов завершила свою работу requests.IsCompleted == true");
		}

		public Task CreateBroadcastPublication(DeliveryInfo deliveryInfo, TimeSpan? timeout = null) {
			if (!deliveryItems.IsCompleted) {
				var tsc = new TaskCompletionSource<object>();
				var timer = CreateTimer(timeout);

				var publishItem = new PublishItem(
					deliveryInfo, 
					errorAction: (e)=> {
						timer.Enabled = false;
						tsc?.SetException(e);
					}, 
					successAction: () => {
						timer.Enabled = false;
						tsc?.SetResult(null);
					});

				timer.Elapsed += (object source, ElapsedEventArgs e) => {
					timer.Enabled = false;
					tsc?.SetException(new OperationCanceledException($"Task timeout canceled."));
					publishItem.IsCanceled = true;
				};

				Task.Factory.StartNew(()=>deliveryItems.Add(publishItem));

				return tsc.Task;
			}
			throw new InvalidOperationException("Очередь запросов завершила свою работу requests.IsCompleted == true");
		}



		private ResponseMessage CreateResponse(DeliveryInfo deliveryInfo, System.Timers.Timer timer) {
			var task = new ResponseTask(timer);
			var resp = new ResponseMessage(task, deliveryInfo.CorrelationId);

			responseMessageHandler.AddSubscription(deliveryInfo.CorrelationId, resp);

			return resp;
		}

		private System.Timers.Timer CreateTimer(TimeSpan? timeout) {
			timeout = timeout ?? new TimeSpan().Add(rmqConfig.RequestTimeout);

			var timer = new System.Timers.Timer(timeout.Value.TotalMilliseconds) {
				Enabled = true
			};

			return timer;
		}
		public void Dispose() {
			deliveryItems.CompleteAdding();
			channel.Close();
		}
	}
}
