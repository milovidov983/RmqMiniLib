using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class BasicPublisher : IPublisher, IDisposable {
		private readonly IChannelPool channelPool;
		private readonly BlockingCollection<DeliveryInfo> requests = new BlockingCollection<DeliveryInfo>();
		

		public BasicPublisher( IChannelPool channelPool) {
			this.channelPool = channelPool;
				
			RequestHandlerStartMainLoop().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Запускаем прослушивание коллекции в которую попадают данные на отправку в рамках RPC вызова
		/// Если во врем отправки происходит ошибка то переподключаемся и персоздаем все каналы
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



		public DeliveredMessage Publish(DeliveryInfo deliveryInfo) {
			if (!requests.IsCompleted) {
				requests.Add(deliveryInfo);
			}
			return deliveryInfo.DeliveredMessage;
		}

		public void Dispose() {
			requests.CompleteAdding();
			var channel = channelPool.GetChannel();
			channel.Close();
		}
	}


	internal interface IReplyHandler {
		void AddReplySubscription(string correlationId, DeliveredMessage resonseHandler);
		Task ReceiveReply(object model, BasicDeliverEventArgs ea);
		DeliveredMessage RemoveReplySubscription(string correlationId);
	}
}
