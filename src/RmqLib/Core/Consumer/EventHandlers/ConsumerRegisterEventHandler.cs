using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ConsumerRegisterEventHandler : IConsumerRegisterEventHandler {
		private readonly IRmqLogger logger;

		private readonly ConcurrentBag<Action<object, ConsumerEventArgs>> consumerRegisterEventHandlers
			= new ConcurrentBag<Action<object, ConsumerEventArgs>>();

		public ConsumerRegisterEventHandler(IRmqLogger logger) {
			this.logger = logger;
		}

		public void AddHandler(Action<object, ConsumerEventArgs> handler) {
			consumerRegisterEventHandlers.Add(handler);
		}

		public void RegisteredHandler(object sender, ConsumerEventArgs e) {
			logger.Debug($"Consumer registered event happened.");

			consumerRegisterEventHandlers.ToList().ForEach(handler => {
				try {
					handler.Invoke(sender, e);
				} catch (Exception ex) {
					logger.Error($"Consumer registered event handler error: {ex.Message}");
				}
			});
		}
	}
}
