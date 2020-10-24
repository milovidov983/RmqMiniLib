using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib.Core {

	internal class PublishStatus {
		public bool IsSuccess { get; set; }
		public Exception Error { get; set; }

		public static PublishStatus SuccessStatus = new PublishStatus {
			IsSuccess = true
		};
	}


	internal class ChannelWrapper : IChannelWrapper {
		private readonly IModel channel;
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);


		public ChannelWrapper(IModel channel) {
			this.channel = channel;
		}

		/// <summary>
		/// Вызвать при потере соединения
		/// </summary>
		public void LockChannel() {
			semaphore.Wait();
		}

		/// <summary>
		/// Восстановить доступ к каналу после регистрации consumer
		/// </summary>
		public void UnlockChannel() {
			semaphore.Release();
		}

		public async Task<PublishStatus> BasicPublish(PublishItem publishItem) {
			await semaphore.WaitAsync();
			if (publishItem.IsCanceled) {
				return PublishStatus.SuccessStatus;
			}
			try {
				var deliveryInfo = publishItem.DeliveryInfo;

				var props = channel.CreateBasicProperties();
				props.CorrelationId = deliveryInfo.CorrelationId;
				props.ReplyTo = deliveryInfo.ReplyTo;
				props.AppId = DeliveryInfo.AppId;

				channel.BasicPublish(
					exchange: DeliveryInfo.ExhangeName,
					routingKey: deliveryInfo.Topic,
					basicProperties: props,
					body: deliveryInfo.Body
					);

				return PublishStatus.SuccessStatus;
			} catch (Exception ex) {
				return new PublishStatus {
					IsSuccess = false,
					Error = ex
				};
			} finally {
				semaphore.Release();
			}
		}

		public void Close() {
			try {
				semaphore.Wait();
				if (!channel.IsClosed) {
					channel.Close();
				}
			} finally {
				semaphore.Release();
			}
		}

	}
}
