using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib.Core {


	internal class ChannelWrapper : IChannelWrapper {
		private readonly IModel channel;
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

		public IModel GetTrueChannel() {
			return channel;
		}

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

		public async Task BasicPublish(string exchange, string routingKey, IBasicProperties basicProperties, byte[] body) {
			try {
				await semaphore.WaitAsync();
				channel.BasicPublish(
					exchange: exchange,
					routingKey: routingKey,
					basicProperties: basicProperties,
					body: body);
			} finally {
				semaphore.Release();
			}
		}

		public async Task BasicAck(ulong deliveryTag, bool multiple) {
			try {
				await semaphore.WaitAsync();
				channel.BasicAck(deliveryTag, multiple);
			} finally {
				semaphore.Release();
			}
		}		
		
		public async Task BasicReject(ulong deliveryTag, bool requeue) {
			try {
				await semaphore.WaitAsync();
				channel.BasicReject(deliveryTag, requeue);
			} finally {
				semaphore.Release();
			}
		}

		public async Task QueueBind(string queue, string exchange, string routingKey) {
			try {
				await semaphore.WaitAsync();
				channel.QueueBind(queue,exchange,routingKey);
			} finally {
				semaphore.Release();
			}
		}


		public void Close() {
			try {
				semaphore.Wait();
				if (!channel.IsClosed) {
					channel.Abort();
				}
			} finally {
				semaphore.Release();
			}
		}

		public async Task BasicConsume(IBasicConsumer consumer, string queue, bool autoAck) {
			try {
				await semaphore.WaitAsync();
				channel.BasicConsume(
					queue: queue,
					autoAck: autoAck,
					consumer: consumer);
				
			} finally {
				semaphore.Release();
			}
		}

		public async Task<IBasicProperties> CreateBasicProperties() {
			try {
				await semaphore.WaitAsync();
				return channel.CreateBasicProperties();
			} finally {
				semaphore.Release();
			}
		}

		
	}
}
