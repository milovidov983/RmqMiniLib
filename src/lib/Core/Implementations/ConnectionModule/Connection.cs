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


		public bool IsConnected { get=> RmqConnection?.IsOpen == true; }
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
		private EventHandlers connectionEventHandlers;
		/// <summary>
		/// TODO comment
		/// </summary>
		private ILogger logger;
		/// <summary>
		/// TODO comment
		/// </summary>
		public Connection(RmqConfig rmqConfig, EventHandlers connectionEventHandlers, ILogger logger) {
			this.rmqConfig = rmqConfig;
			this.connectionEventHandlers = connectionEventHandlers;
			this.logger = logger;
		}
		/// <summary>
		/// TODO comment
		/// </summary>
		public void StartConnection(bool reconnectIfFailed = true) {
			InitConnectionFactory();

			try {
				CreateConnection();
			} catch (Exception e) {
				var message = $"Failed connect to the RabbitMQ: {e.Message} ";
				if (reconnectIfFailed) {
					logger?.LogError(message + ", try reconnect...");
					RetryConnection();
				} else {
					logger?.LogError(message + ", application shutdown...");
					throw;
				}
			}
		}

		private void InitConnectionFactory() {
			factory = new ConnectionFactory {
				HostName = rmqConfig.HostName,
				Password = rmqConfig.Password,
				UserName = rmqConfig.UserName
			};
			factory.DispatchConsumersAsync = true;
			factory.AutomaticRecoveryEnabled = true;
		}

		public void CreateConnection() {
			RmqConnection = factory.CreateConnection();
			BindEventHandlers();
		}

		private void BindEventHandlers() {
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
					CreateConnection();
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
