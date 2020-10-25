using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RmqLib.Core {
	class ConnectionEventHandlers : IConnectionEventHandlers {
		private readonly ConcurrentBag<Action<object, ShutdownEventArgs>> connectionShutdownEventHandlers 
			= new ConcurrentBag<Action<object, ShutdownEventArgs>>();

		private readonly IConnectionWrapper connectionWrapper;
		private readonly IRmqLogger logger;


		public ConnectionEventHandlers(IConnectionWrapper connectionWrapper, IRmqLogger logger) {
			this.connectionWrapper = connectionWrapper;
			this.logger = logger;
			Init();
		}

		void Init() {

			connectionWrapper.BindEventHandler(c => {
				try {
					c.ConnectionShutdown += ConnectionShutdownEventHandler;
					c.ConnectionBlocked += ConnectionBlockedHandler;
					c.CallbackException += CallbackExceptionHandler;
					c.ConnectionUnblocked += ConnectionUnblockedHandler;

					logger.Debug($"{nameof(ConnectionEventHandlers)} binded to connection event handlers");
				} catch (Exception e) {
					logger.Error($"{nameof(ConnectionEventHandlers)} error to bind connection event handlers: {e.Message}");
				}
			});

			connectionWrapper.RegisterUnsubscribeHandler(c => {
				if (this is null) {
					return;
				}
				c.ConnectionShutdown -= ConnectionShutdownEventHandler;
				c.ConnectionBlocked -= ConnectionBlockedHandler;
				c.CallbackException -= CallbackExceptionHandler;
				c.ConnectionUnblocked -= ConnectionUnblockedHandler;
			});
		}

		private void ConnectionUnblockedHandler(object sender, EventArgs e) {
			logger.Debug($"{nameof(ConnectionEventHandlers)} ConnectionShutdownEvent event happened ");
		}

		private void CallbackExceptionHandler(object sender, CallbackExceptionEventArgs e) {
			logger.Debug($"{nameof(ConnectionEventHandlers)} CallbackException event happened " +
			$"{nameof(CallbackExceptionEventArgs.Exception)} {e.Exception?.Message} " +
			$"{nameof(CallbackExceptionEventArgs.Detail)}  {string.Join( " ",  e.Detail?.Select(kv => ($"{kv.Key}: {kv.Value}") ) ) } ");
		}

		private void ConnectionBlockedHandler(object sender, ConnectionBlockedEventArgs e) {
			logger.Debug($"{nameof(ConnectionEventHandlers)} ConnectionBlocked event happened " +
			$"{nameof(ConnectionBlockedEventArgs.Reason)} {e.Reason}");
		}



		public void AddHandler(Action<object, ShutdownEventArgs> shutdownHandler) {
			connectionShutdownEventHandlers.Add(shutdownHandler);
		}

		private void ConnectionShutdownEventHandler(object sender, ShutdownEventArgs e) {
			logger.Debug($"{nameof(ConnectionEventHandlers)} ConnectionShutdownEvent event happened " +
				$"{nameof(ShutdownEventArgs.ReplyText)} {e.ReplyText}");

			connectionShutdownEventHandlers.ToList().ForEach(handler => {
				try {
					handler.Invoke(sender, e);
				} catch(Exception ex) {
					logger.Error($"{nameof(ConnectionEventHandlers)} " +
						$"ConnectionShutdownEvent handler error: {ex.Message}");
				}
			});
		}
	}
}
