using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RmqLib.Core {
	/// <summary>
	/// TODO comment
	/// </summary>
	public class ConnectionService: IConnectionService {
		/// <summary>
		/// TODO comment
		/// </summary>
		public IConnection RmqConnection { get; private set; }
		/// <summary>
		/// 
		/// </summary>
		public bool IsConnected { get=> RmqConnection?.IsOpen == true; }
		/// <summary>
		/// TODO comment
		/// </summary>
		private RabbitMQ.Client.ConnectionFactory factory;
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly RmqConfig rmqConfig;
		/// <summary>
		/// 
		/// </summary>
		private readonly IConnectionEvents connectionEvents;
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly ILogger logger;
		/// <summary>
		/// 
		/// </summary>
		private readonly IRetryConnection retryConnection;

		/// <summary>
		/// TODO comment
		/// </summary>
		internal ConnectionService(
			RmqConfig rmqConfig, 
			IRetryConnectionFactory retryConnectionFactory,
			IConnectionEvents connectionEvents,
			ILogger logger) {
			this.rmqConfig = rmqConfig;
			this.connectionEvents = connectionEvents;
			this.logger = logger;
			this.retryConnection = retryConnectionFactory.Create(this);
		}
		/// <summary>
		/// TODO comment
		/// </summary>
		public void StartConnection(bool reconnectIfFailed = true) {
			InitConnectionFactory();

			try {
				CreateRmqConnection();
			} catch (Exception e) {
				var message = $"Failed connect to the RabbitMQ: {e.Message} ";
				if (reconnectIfFailed) {
					logger?.LogError(message + ", try reconnect...");
					retryConnection.Retry();
				} else {
					logger?.LogError(message + ", application shutdown...");
					throw;
				}
			}
		}

		private void InitConnectionFactory() {
			factory = new RabbitMQ.Client.ConnectionFactory {
				HostName = rmqConfig.HostName,
				Password = rmqConfig.Password,
				UserName = rmqConfig.UserName
			};
			factory.DispatchConsumersAsync = true;
			factory.AutomaticRecoveryEnabled = true;
		}

		public void CreateRmqConnection() {
			RmqConnection = factory.CreateConnection();
			connectionEvents.BindEventHandlers(this);
		}
	}
}
