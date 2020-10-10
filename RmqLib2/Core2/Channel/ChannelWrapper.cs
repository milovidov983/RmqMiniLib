﻿using RabbitMQ.Client;
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
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);


		public ChannelWrapper(IModel channel) {
			this.channel = channel;
		}

		/// <summary>
		/// Вызвать при потере соединения
		/// </summary>
		public async Task LockChannel() {
			await semaphore.WaitAsync();
		}		
		
		/// <summary>
		/// Восстановить доступ к каналу после регисттрации consumer
		/// </summary>
		public void UnlockChannel() {
			semaphore.Release();
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
			if (!channel.IsClosed) {
				this.channel.Close();
			}
			semaphore.Release();
		}

	}
}