using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib {
	/// <summary>
	/// Управляет потребителем для RPC ответов, 
	/// в случае нештатных ситуаций пытается пересоздать и привязать обработчики повторно
	/// </summary>
	internal class ConsumerManager : IConsumerManager {
		private IConsumerFactory consumerFactory;
		private readonly IConsumerBinder consumerBinder;
		private AsyncEventingBasicConsumer consumerInstance;

		private readonly IRmqLogger logger;

		private List<Action<AsyncEventingBasicConsumer>> unsubscribeEventHandlers
			= new List<Action<AsyncEventingBasicConsumer>>();

		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

		public ConsumerManager(
			IConsumerFactory consumerFactory,
			IConsumerBinder consumerBinder,
			IRmqLogger logger) {
			this.consumerFactory = consumerFactory;
			this.consumerBinder = consumerBinder;
			this.logger = logger;
		}


		public void BindEventHandlers(Action<AsyncEventingBasicConsumer> action) {
			action.Invoke(consumerInstance);
		}




		public void RegisterUnsubscribeAction(Action<AsyncEventingBasicConsumer> action) {
			unsubscribeEventHandlers.Add(action);
		
		}

		public void InitConsumer() {
			Log($"Создаем потребителя отвечающего за прослушивание RPC ответов.");


			consumerInstance = consumerFactory.CreateBasicConsumer();


			Log($"Создан потребитель для получения ответов от RPC вызовов.");

			
			consumerInstance.Registered += Registered;
			consumerInstance.Shutdown += Shutdown;
			consumerInstance.Unregistered += Unregistered;
			consumerInstance.ConsumerCancelled += ConsumerCancelled;

			consumerBinder.Bind(consumerInstance);
		}



		private Task Registered(object sender, ConsumerEventArgs @event) {
			Log($"Потребитель ответов RPC успешно подключен к шине.");
			return Task.CompletedTask;
		}

		private void Unsubscribe() {
			Log($"Отписываем обработчиков от событий потребителя.");
			//consumerInstance.Received -= responseMessageHandler.HandleMessage;
			//consumerInstance.Registered -= connectionManager.ConsumerRegistred;
			consumerInstance.Registered -= Registered;

			consumerInstance.Shutdown -= Shutdown;
			consumerInstance.Unregistered -= Unregistered;
			consumerInstance.ConsumerCancelled -= ConsumerCancelled;

			unsubscribeEventHandlers.ForEach(action => action.Invoke(consumerInstance));
		}


		private async Task Recover() {
			// TODO пока не понятно надо ли оно вообще  и будет ли работать
			Log($" ХХХ Восстановление временно отключено. ХХХ ");
			return;

			if (consumerInstance.IsRunning) {
				return;
			}

			await semaphore.WaitAsync();

			if (consumerInstance.IsRunning) {
				return;
			}

			Log($"Пытаемся восстановить потребителя RPC ответов.");

			try {

				Unsubscribe();
				InitConsumer();


			} catch (Exception e) {
				Log($"Ошибка при попытке пересоздать потребителя для RPC ответов: {e.Message}");
			} finally {
				semaphore.Release();
			}

		}

		// TODO перенсти  все обработчики в отдельный класс ConsumerEventHandlers

		private Task Shutdown(object sender, ShutdownEventArgs @event) {
			Log($"ConsumerEvent Shutdown. ReplyText {@event.ReplyText}");
			return Recover();
		}

		private Task Unregistered(object sender, ConsumerEventArgs @event) {
			Log($"ConsumerEvent Unregistered.");
			return Recover();
		}
		private Task ConsumerCancelled(object sender, ConsumerEventArgs @event) {
			Log($"ConsumerEvent ConsumerCancelled. ");
			return Recover();
		}


		private void Log(string msg) {
			logger.Debug($"[{nameof(ConsumerManager)}]: {msg}");
		}


	}
}
