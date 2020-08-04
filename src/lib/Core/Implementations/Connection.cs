using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RmqLib {
	/// <summary>
	/// TODO comment
	/// </summary>
	public class Connection: IConnection {
		/// <summary>
		/// TODO comment
		/// </summary>
		public RabbitMQ.Client.IConnection RmqConnection { get; private set; }

		/// <summary>
		/// TODO comment
		/// </summary>
		private ConnectionFactory factory;
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly RmqConfig rmqConfig;
		/// <summary>
		/// TODO comment
		/// </summary>
		private ConnectionEventHandlers connectionEventHandlers;
		/// <summary>
		/// TODO comment
		/// </summary>
		private ILogger logger;
		/// <summary>
		/// TODO comment
		/// </summary>
		public Connection(RmqConfig rmqConfig, ConnectionEventHandlers connectionEventHandlers, ILogger logger) {
			this.rmqConfig = rmqConfig;
			this.connectionEventHandlers = connectionEventHandlers;
			this.logger = logger;
		}
		/// <summary>
		/// TODO comment
		/// </summary>
		public void ConnectToRmq() {
			factory = new ConnectionFactory {
				HostName = rmqConfig.HostName,
				Password = rmqConfig.Password,
				UserName = rmqConfig.UserName
			};
			factory.DispatchConsumersAsync = true;
			factory.AutomaticRecoveryEnabled = true;
			RmqConnection = factory.CreateConnection();

			if (connectionEventHandlers?.ConnectionShutdown != null) {
				RmqConnection.ConnectionShutdown += connectionEventHandlers.ConnectionShutdown;
			}
			if (connectionEventHandlers?.CallbackException != null) {
				RmqConnection.CallbackException += connectionEventHandlers.CallbackException;
			}
		}
		/// <summary>
		/// TODO comment
		/// </summary>
		public void RetryConnection() {
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
