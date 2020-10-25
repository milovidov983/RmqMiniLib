using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ConsumerReceiveEventHandelr : IConsumerReceiveEventHandelr {
		private readonly IRmqLogger logger;

		private readonly ConcurrentBag<Action<object, BasicDeliverEventArgs>> handlers
			= new ConcurrentBag<Action<object, BasicDeliverEventArgs>>();

		public ConsumerReceiveEventHandelr(IRmqLogger logger) {
			this.logger = logger;
		}

		public void AddHandler(Action<object, BasicDeliverEventArgs> handler) {
			handlers.Add(handler);
		}

		public void ReceiveHandler(object sender, BasicDeliverEventArgs e) {
			// Асинхронно потому что трудоёмкая задача по обработке запроса
			Task.Factory.StartNew(() => {
				handlers.ToList().ForEach(handler => {
					try {
						handler.Invoke(sender, e);
					} catch (Exception ex) {
						logger.Error($"receive handler error: {ex.Message}");
					}
				});
			});
		}
	}
}
