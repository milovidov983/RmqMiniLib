using RabbitMQ.Client;
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
		private readonly IConsumerCommonEventHandelr consumerCommonEventHandelr;


		public ConsumerMainEventHandlers(
			IConsumerEventHandlersFactory consumerEventHandlersFactory, 
			IConsumerManager consumerManager, 
			IRmqLogger logger) {
			this.consumerManager = consumerManager;
			this.logger = logger;

			consumerRegisterEventHandler = consumerEventHandlersFactory.CreateRegisterEventHandler();
			consumerReceiveEventHandelr = consumerEventHandlersFactory.CreateReceiveEventHandelr();
			consumerCommonEventHandelr = consumerEventHandlersFactory.CreateCommonEventHandelr();

			Init();
		}

		private void Init() {
			consumerManager.BindEventHandlers(c => {
				try {
					c.Registered += RegisteredHandler;
					c.Received += Receivehandler;
					// TODO перенести все обработчики из consumerManager

					c.Shutdown += ShutdownHandler;
					c.Unregistered += UnregisteredHandler;
					c.ConsumerCancelled += ConsumerCancelledHandler;

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
				c.Shutdown -= ShutdownHandler;
				c.Unregistered -= UnregisteredHandler;
				c.ConsumerCancelled -= ConsumerCancelledHandler;
			});
		}

		private Task ConsumerCancelledHandler(object sender, ConsumerEventArgs @event) {
			consumerCommonEventHandelr.ConsumerCancelledHandler(sender, @event);
			return Task.CompletedTask;
		}

		private Task UnregisteredHandler(object sender, ConsumerEventArgs @event) {
			consumerCommonEventHandelr.UnregisteredHandler(sender, @event);
			return Task.CompletedTask;
		}

		private Task ShutdownHandler(object sender, ShutdownEventArgs @event) {
			consumerCommonEventHandelr.ShutdownHandler(sender, @event);
			return Task.CompletedTask;
		}

		private Task Receivehandler(object sender, BasicDeliverEventArgs e) {
			consumerReceiveEventHandelr.ReceiveHandler(sender, e);
			return Task.CompletedTask;
		}

		private Task RegisteredHandler(object sender, ConsumerEventArgs e) {
			consumerRegisterEventHandler.RegisteredHandler(sender, e);
			return Task.CompletedTask;
		}



		public void AddReceiveHandler(Action<object, BasicDeliverEventArgs> handler) {
			consumerReceiveEventHandelr.AddHandler(handler);
		}

		public void AddRegisterHandler(Action<object, ConsumerEventArgs> handler) {
			consumerRegisterEventHandler.AddHandler(handler);
		}

	}
}
