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
		private IConsumerFactory consumerFactory;

		private AsyncEventingBasicConsumer consumerInstance;

		private readonly IRmqLogger logger;

		private List<Action<AsyncEventingBasicConsumer>> unsubscribeEventHandlers
			= new List<Action<AsyncEventingBasicConsumer>>();

	
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
