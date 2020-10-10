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




		public Task<PublishStatus> BasicPublish(DeliveryInfo deliveryInfo) {
			var tsc = new TaskCompletionSource<PublishStatus>();
			Task.Factory.StartNew(async () => {
				try {
					await semaphore.WaitAsync();

					var props = channel.CreateBasicProperties();
					props.CorrelationId = deliveryInfo.CorrelationId;
					props.ReplyTo = ServiceConstants.REPLY_QUEUE_NAME;
					props.AppId = deliveryInfo.AppId;

					channel.BasicPublish(
						exchange: deliveryInfo.ExhangeName,
						routingKey: deliveryInfo.Topic,
						basicProperties: props,
						body: deliveryInfo.Body
						);

					tsc.SetResult(new PublishStatus {
						 IsSuccess = true
					});
				} catch (Exception ex) {
					tsc.SetResult(new PublishStatus {
						IsSuccess = false,
						Error = ex.Message
					});
				} finally {
					semaphore.Release();
				}
			});

			return tsc.Task;
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
