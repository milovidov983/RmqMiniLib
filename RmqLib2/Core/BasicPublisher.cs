using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class BasicPublisher : IBasicPublisher, IPublisher, IDisposable {
		private Channel channel;
		private readonly BlockingCollection<DeliveryInfo> requests = new BlockingCollection<DeliveryInfo>();
		private readonly IConnectionManager connectionManager;
		

		public BasicPublisher(IConnectionManager connectionManager, IModel chanel) {
			this.connectionManager = connectionManager;
			InitChanel(chanel);
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
				var publishStatus = await channel.BasicPublish(deliveryInfo);

				if (!publishStatus.IsSuccess) {
					Console.WriteLine($"{nameof(BasicPublisher)}{nameof(RequestHandlerStartMainLoop)} {publishStatus.Error}");
					
					connectionManager.Reconnect();

					requests.TryAdd(deliveryInfo);
				}
				deliveryInfo.S
			}
		}

		public Task InitChanel(IModel channel) {
			return this.channel.SetChannel(channel);
		}

		public void Publish(DeliveryInfo deliveryInfo) {
			if (!requests.IsCompleted) {
				requests.Add(deliveryInfo);
			}
		}

		public void Dispose() {
			requests.CompleteAdding();
			channel.Close();
		}
	}


	internal interface IReplyHandler {
		Task ReceiveReply(object model, BasicDeliverEventArgs ea);
	}
}
