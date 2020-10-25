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
	internal class ConsumerManager : IConsumerManager, IDisposable {
		private readonly IConsumerFactory consumerFactory;
		private readonly IRmqLogger logger;
		private readonly List<Action<AsyncEventingBasicConsumer>> unsubscribeEventHandlers
			= new List<Action<AsyncEventingBasicConsumer>>();

		private AsyncEventingBasicConsumer consumerInstance;



	
		public ConsumerManager(
			IConsumerFactory consumerFactory,
			IRmqLogger logger) {
			this.consumerFactory = consumerFactory;
			this.logger = logger;
		}


		public void BindEventHandlers(Action<AsyncEventingBasicConsumer> action) {
			action.Invoke(consumerInstance);
		}

		public void RegisterUnsubscribeAction(Action<AsyncEventingBasicConsumer> action) {
			unsubscribeEventHandlers.Add(action);
		
		}

		public void InitConsumer() {
			logger.Info($"Пробуем создать consumer...");

			// придётся пилить свой враппер вокруг AsyncEventingBasicConsumer
			// тк интерфейсы не содержат событий IAsyncBasicConsumer
			// а явно зависеть от библиотечной имплементации не очень
			consumerInstance = consumerFactory.CreateBasicConsumer(); 
			logger.Info($"Consumer создан успешно");
		}

		private void Unsubscribe() {
			unsubscribeEventHandlers.ForEach(action => action?.Invoke(consumerInstance));
		}


		public void Dispose() {
			Unsubscribe();
		}
	}
}
