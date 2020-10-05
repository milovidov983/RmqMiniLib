using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib2 {

	internal class PublishStatus {
		public bool IsSuccess { get; set; }
		public string Error { get; set; }
	}


	internal class ChannelWrapper : IChannelWrapper {
		private IModel channel;
		IReplyHandler replyHandler;
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

		public ChannelWrapper(IModel channel, IReplyHandler replyHandler) {
			this.channel = channel;
			this.replyHandler = replyHandler;
		}

		public async Task SetChannel(IModel channel) {
			await semaphore.WaitAsync();
			this.channel = channel ?? throw new ArgumentNullException(nameof(channel));
			BindReplyHandler(channel);
			semaphore.Release();
		}
		private void BindReplyHandler(IModel channel) {
			var consumer = new AsyncEventingBasicConsumer(channel);
			channel.BasicConsume(
				consumer: consumer,
				queue: ServiceConstants.REPLY_QUEUE_NAME,
				autoAck: true);
			consumer.Received += replyHandler.ReceiveReply;
		}

		public async Task<PublishStatus> BasicPublish(DeliveryInfo deliveryInfo) {
			try {
				await semaphore.WaitAsync();

				var props = channel.CreateBasicProperties();
				props.CorrelationId = deliveryInfo.CorrelationId;
				props.ReplyTo = ServiceConstants.REPLY_QUEUE_NAME;

				channel.BasicPublish(
					exchange: deliveryInfo.ExhangeName,
					routingKey: deliveryInfo.Topic,
					basicProperties: props,
					body: deliveryInfo.Body
					);

				return new PublishStatus {
					IsSuccess = true
				};
			} catch (Exception ex) {
				// TODO Протестировать и определить типы ошибок , выделить те что касаются сети 
				// реагировать на сетевые переподключением, на некоторые ретраем?


				return new PublishStatus {
					IsSuccess = false,
					Error = ex.Message
				};
			} finally {
				semaphore.Release();
			}
		}

		public void Close() {
			semaphore.WaitAsync().GetAwaiter().GetResult();
			this.channel.Close();
			semaphore.Release();
		}

	}
}
