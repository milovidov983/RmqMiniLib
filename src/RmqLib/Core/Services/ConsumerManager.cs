using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib2 {
	/// <summary>
	/// Управляет потребителем для RPC ответов, 
	/// в случае нештатных ситуаций пытается пересоздать и привязать обработчики повторно
	/// </summary>
	internal class ConsumerManager {
		private IModel channel;
		private IReplyHandler replyHandler;
		private IConnectionManager channelRecovery;

		private AsyncEventingBasicConsumer consumerInstance;

		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

		public ConsumerManager(
			IModel channel, 
			IReplyHandler replyHandler, 
			IConnectionManager channelRecovery) {
			this.channel = channel;
			this.replyHandler = replyHandler;
			this.channelRecovery = channelRecovery;
		}

		public void InitConsumer() {
			Log($"Создаем потребителя отвечающего за прослушивание RPC ответов.");
			consumerInstance = new AsyncEventingBasicConsumer(channel);

			channel.BasicConsume(
				consumer: consumerInstance,
				queue: ServiceConstants.REPLY_QUEUE_NAME,
				autoAck: true);

			Log($"Создан потребитель для получения ответов от RPC вызовов.");

			consumerInstance.Received += replyHandler.ReceiveReply;
			consumerInstance.Registered += channelRecovery.ConsumerRegistred;
			consumerInstance.Registered += Registered;

			consumerInstance.Shutdown += Shutdown;
			consumerInstance.Unregistered += Unregistered;
			consumerInstance.ConsumerCancelled += ConsumerCancelled;
		}

		private Task Registered(object sender, ConsumerEventArgs @event) {
			Log($"Потребитель ответов RPC успешно подключен к шине.");
			return Task.CompletedTask;
		}

		private void Unsubscribe() {
			Log($"Отписываем обработчиков от событий потребителя.");
			consumerInstance.Received -= replyHandler.ReceiveReply;
			consumerInstance.Registered -= channelRecovery.ConsumerRegistred;
			consumerInstance.Registered -= Registered;

			consumerInstance.Shutdown -= Shutdown;
			consumerInstance.Unregistered -= Unregistered;
			consumerInstance.ConsumerCancelled -= ConsumerCancelled;
		}




		private async Task Recover() {
			if (!consumerInstance.IsRunning) {
				await semaphore.WaitAsync();

				try {
					Log($"Пытаемся восстановить потребителя RPC ответов.");

					Unsubscribe();
					InitConsumer();

				}catch(Exception e) {
					Log($"Ошибка при попытке пересоздать потребителя для RPC ответов: {e.Message}");
				} finally {
					semaphore.Release();
				}
			}
		}

		private Task Shutdown(object sender, ShutdownEventArgs @event) {
			Log($"Shutdown.... reply text: {@event.ReplyText}");
			return Recover();
		}

		private Task Unregistered(object sender, ConsumerEventArgs @event) {
			Log($"Unregistered.... ");
			return Recover();
		}
		private Task ConsumerCancelled(object sender, ConsumerEventArgs @event) {
			Log($"ConsumerCancelled.... ");
			return Recover();
		}


		private void Log(string msg) {
			Console.WriteLine($"[{nameof(ConsumerManager)}]: {msg}");
		}


	}
}
