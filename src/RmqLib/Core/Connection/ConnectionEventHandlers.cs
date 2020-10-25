using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RmqLib.Core.Connection {
	class ConnectionEventHandlers : IConnectionEventHandlers {
		private readonly List<Action<IConnection>> activeEventHandlers = new List<Action<IConnection>>();

		private readonly ConcurrentBag<Action<IConnection>> unsubscribeEventHandlers = new ConcurrentBag<Action<IConnection>>();

		private readonly ConcurrentBag<Action<object, ShutdownEventArgs>> connectionShutdownEventHandlers = new ConcurrentBag<Action<object, ShutdownEventArgs>>();

		private readonly IConnectionWrapper connectionWrapper;
		private readonly IRmqLogger logger;

		

		public ConnectionEventHandlers(IConnectionWrapper connectionWrapper) {
			this.connectionWrapper = connectionWrapper;

			connectionWrapper.RegisterUnsubscribeAction(c => { UnsubscribeAll(); });
		}

		private void UnsubscribeAll() {
			throw new NotImplementedException();
		}

		public void AddEventHandler(Action<ICollection> initAction) {

		}

		void Init() {

			connectionWrapper.SetSettings(c => {
				try {
					c.ConnectionShutdown += ConnectionShutdownEventHandler;
					c.ConnectionBlocked += ConnectionBlockedHandler;
					c.CallbackException += CallbackExceptionHandler;
					c.ConnectionUnblocked += ConnectionUnblockedHandler;

					logger.Debug($"{nameof(ConnectionEventHandlers)} binded to connection shutdown event");
				} catch (Exception e) {
					logger.Error($"{nameof(ConnectionEventHandlers)} error to bind to connection shutdown event: {e.Message}");
				}
			});

			connectionWrapper.RegisterUnsubscribeAction(c => {
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
			$"{nameof(e.Exception)} {e.Exception?.Message} " +
			$"{nameof(e.Detail)}  {string.Join( " ",  e.Detail?.Select(kv => ($"{kv.Key}: {kv.Value}") ) ) } ");
		}

		private void ConnectionBlockedHandler(object sender, ConnectionBlockedEventArgs e) {
			logger.Debug($"{nameof(ConnectionEventHandlers)} ConnectionBlocked event happened " +
			$"{nameof(e.Reason)} {e.Reason}");
		}

		public void AddHandler(Action<object, ShutdownEventArgs> shutdownHandler) {
			connectionShutdownEventHandlers.Add(shutdownHandler);
		}

		private void ConnectionShutdownEventHandler(object sender, ShutdownEventArgs e) {
			logger.Debug($"{nameof(ConnectionEventHandlers)} ConnectionShutdownEvent event happened " +
				$"{nameof(e.ReplyText)} {e.ReplyText}");

			connectionShutdownEventHandlers.ToList().ForEach(handler => {
				try {
					handler.Invoke(sender, e);
				} catch(Exception e) {
					logger.Error($"{nameof(ConnectionEventHandlers)} " +
						$"ConnectionShutdownEvent handler error: {e.Message}");
				}
			});
		}
	}
}
