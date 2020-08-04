using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RmqLib.Core {
	/// <summary>
	/// Обработчики событий соединения с RMQ
	/// </summary>
	public class ConnectionEventHandlers {
		/// <summary>
		/// Raised when the connection is destroyed.
		/// </summary>
		public EventHandler<ShutdownEventArgs> ConnectionShutdown;
		/// <summary>
		///  Signalled when an exception occurs in a callback invoked by the connection.
		/// </summary>
		public EventHandler<CallbackExceptionEventArgs> CallbackException;
	}

	public interface IConnection {
		RabbitMQ.Client.IConnection RmqConnection { get; }
	}

	public class Connection: IConnection {
		public RabbitMQ.Client.IConnection RmqConnection { get; private set; }
		private ConnectionFactory factory;
		private readonly RmqConfig rmqConfig;
		private ConnectionEventHandlers connectionEventHandlers;
		private ILogger logger;

		public Connection(RmqConfig rmqConfig, ConnectionEventHandlers connectionEventHandlers, ILogger logger) {
			this.rmqConfig = rmqConfig;
			this.connectionEventHandlers = connectionEventHandlers;
			this.logger = logger;
		}

		private void ConnectToRmq() {
			factory = new ConnectionFactory {
				HostName = rmqConfig.HostName,
				Password = rmqConfig.Password,
				UserName = rmqConfig.UserName
			};
			factory.DispatchConsumersAsync = true;
			factory.AutomaticRecoveryEnabled = true;
			RmqConnection = factory.CreateConnection();

			if (connectionEventHandlers.ConnectionShutdown != null) {
				RmqConnection.ConnectionShutdown += connectionEventHandlers.ConnectionShutdown;
			}
			if (connectionEventHandlers.CallbackException != null) {
				RmqConnection.CallbackException += connectionEventHandlers.CallbackException;
			}
		}

		private void RetryConnection() {
			int timeoutMs = 1000;
			var d = 1;
			while (!RmqConnection.IsOpen) {
				Thread.Sleep(timeoutMs);
				try {
					ConnectToRmq();
				} catch (Exception ex) {
					timeoutMs += 1000 / d++;
					logger?.LogWarning(
						$"Reconnection failed: {ex.Message}, " +
						$"the next attempt will reconnect in {timeoutMs / 1000} seconds");
				}
			}
		}


	}
}
