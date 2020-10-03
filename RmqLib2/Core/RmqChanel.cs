using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class RmqChanel : IRmqChanel, IDisposable {
		private IModel chanel;
		private readonly BlockingCollection<Request> requests = new BlockingCollection<Request>();
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
		private readonly IConnectionManager connectionManager;

		public RmqChanel(IConnectionManager connectionManager, IModel chanel) {
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
			Request request = default;
			try {
				while (!requests.IsCompleted) {
					request = requests.Take();

					await semaphore.WaitAsync();

					var props = chanel.CreateBasicProperties();
					props.CorrelationId = request.DeliveryInfo.CorrelationId;
					props.ReplyTo = ServiceConstants.REPLY_QUEUE_NAME;

					chanel.BasicPublish(
						exchange: request.DeliveryInfo.ExhangeName,
						routingKey: request.DeliveryInfo.Topic,
						basicProperties: props,
						request.Payload.Body
						);
				}
			} catch(Exception e) {
				//DEBUG
				Console.WriteLine($"{nameof(RmqChanel)}{nameof(RequestHandlerStartMainLoop)} {e.Message}");

				await connectionManager.Reconnect();
				if (!requests.IsCompleted) {
					requests.Add(request);
				}
			} finally {
				semaphore.Release();
			}
		}

		public void InitChanel(IModel chanel) {
			this.chanel = chanel;
		}

		public void Send(DeliveryInfo deliveryInfo, Payload payload) {
			var request = new Request(deliveryInfo, payload);
			if (!requests.IsCompleted) {
				requests.Add(request);
			}
		}

		public void Dispose() {
			requests.CompleteAdding();
			semaphore.Wait();
			chanel.Close();
		}
	}
}
