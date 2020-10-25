using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	// TODO перенести все обработчики из consumerManager
	internal class ConsumerEventHandlers : IConsumerEventHandlers {
		private readonly IConsumerManager consumerManager;
		private readonly IRmqLogger logger;
		private readonly ConcurrentBag<Action<object, ConsumerEventArgs>> consumerRegisterEventHandlers
			= new ConcurrentBag<Action<object, ConsumerEventArgs>>();

		public ConsumerEventHandlers(IConsumerManager consumerManager, IRmqLogger logger) {
			this.consumerManager = consumerManager;
			this.logger = logger;

			Init();
		}

		void Init() {

			consumerManager.BindEventHandlers(c => {
				try {
					c.Registered += RegisteredHandler;
					// TODO перенести все обработчики из consumerManager

					logger.Debug($"{nameof(ConsumerEventHandlers)} binded to connection shutdown event");
				} catch (Exception e) {
					logger.Error($"{nameof(ConsumerEventHandlers)} error to bind to connection shutdown event: {e.Message}");
				}
			});

			consumerManager.RegisterUnsubscribeAction(c => {
				if (this is null) {
					return;
				}
				c.Registered -= RegisteredHandler;
			});
		}

		public void AddHandler(Action<object, ConsumerEventArgs> handler) {
			consumerRegisterEventHandlers.Add(handler);
		}


		private Task RegisteredHandler(object sender, ConsumerEventArgs e) {
			logger.Debug($"{nameof(ConsumerEventHandlers)} Registered event happened ");

			consumerRegisterEventHandlers.ToList().ForEach(handler => {
				try {
					handler.Invoke(sender, e);
				} catch (Exception ex) {
					logger.Error($"{nameof(ConsumerEventHandlers)} " +
						$"Registered handler error: {ex.Message}");
				}
			});

			return Task.CompletedTask;
		}
	}
}
