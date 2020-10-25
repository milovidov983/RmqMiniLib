using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	// TODO перенести все обработчики из consumerManager
	internal class ConsumerMainEventHandlers : IConsumerMainEventHandlers {
		private readonly IConsumerManager consumerManager;
		private readonly IRmqLogger logger;


		private readonly IConsumerRegisterEventHandler consumerRegisterEventHandler;
		private readonly IConsumerReceiveEventHandelr consumerReceiveEventHandelr;


		public ConsumerMainEventHandlers(
			IConsumerEventHandlersFactory consumerEventHandlersFactory, 
			IConsumerManager consumerManager, 
			IRmqLogger logger) {
			this.consumerManager = consumerManager;
			this.logger = logger;

			consumerRegisterEventHandler = consumerEventHandlersFactory.CreateRegisterEventHandler();
			consumerReceiveEventHandelr = consumerEventHandlersFactory.CreateReceiveEventHandelr();

			Init();
		}

		private void Init() {
			consumerManager.BindEventHandlers(c => {
				try {
					c.Registered += RegisteredHandler;
					c.Received += Receivehandler;
					// TODO перенести все обработчики из consumerManager

					logger.Debug($"binded event handlers");
				} catch (Exception e) {
					logger.Error($"error to bind event handlers: {e.Message}");
				}
			});

			consumerManager.RegisterUnsubscribeAction(c => {
				if (this is null) {
					return;
				}
				c.Received -= Receivehandler;
				c.Registered -= RegisteredHandler;
			});
		}


		public void AddHandler(Action<object, BasicDeliverEventArgs> handler) {
			consumerReceiveEventHandelr.AddHandler(handler);
		}

		public void AddHandler(Action<object, ConsumerEventArgs> handler) {
			consumerRegisterEventHandler.AddHandler(handler);
		}

		private Task Receivehandler(object sender, BasicDeliverEventArgs e) {
			consumerReceiveEventHandelr.ReceiveHandler(sender, e);
			return Task.CompletedTask;
		}

		private Task RegisteredHandler(object sender, ConsumerEventArgs e) {
			consumerRegisterEventHandler.RegisteredHandler(sender, e);
			return Task.CompletedTask;
		}
	}
}
