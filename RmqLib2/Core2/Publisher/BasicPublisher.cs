using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class BasicPublisher : IPublisher, IDisposable {
		private readonly IChannelFactory channelFactory;
		private readonly BlockingCollection<DeliveryInfo> requests = new BlockingCollection<DeliveryInfo>();
		private readonly IReconnectionManager reconnectionManager;
		

		public BasicPublisher(IReconnectionManager reconnectionManager, IChannelFactory channelFactory) {
			this.reconnectionManager = reconnectionManager;
			this.channelFactory = channelFactory;
				
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
				var channel = channelFactory.GetChannel();
				var publishStatus = await channel.BasicPublish(deliveryInfo);

				if (!publishStatus.IsSuccess) {
					Console.WriteLine($"{nameof(BasicPublisher)}{nameof(RequestHandlerStartMainLoop)} {publishStatus.Error}");
					
					reconnectionManager.Reconnect();

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
			var channel = channelFactory.GetChannel();
			channel.Close();
		}
	}


	internal interface IReplyHandler {
		Task ReceiveReply(object model, BasicDeliverEventArgs ea);
	}
}
