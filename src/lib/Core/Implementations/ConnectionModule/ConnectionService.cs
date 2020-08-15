using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RmqLib {
	/// <summary>
	/// TODO comment
	/// </summary>
	public class ConnectionService: IConnectionService {
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
		private ILogger logger;
		/// <summary>
		/// 
		/// </summary>
		private IRetryConnection retryConnection;

		/// <summary>
		/// TODO comment
		/// </summary>
		internal ConnectionService(RmqConfig rmqConfig, IRetryConnectionFactory retryConnectionFactory, ILogger logger) {
			this.rmqConfig = rmqConfig;
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
			factory = new ConnectionFactory {
				HostName = rmqConfig.HostName,
				Password = rmqConfig.Password,
				UserName = rmqConfig.UserName
			};
			factory.DispatchConsumersAsync = true;
			factory.AutomaticRecoveryEnabled = true;
		}

		public void CreateRmqConnection() {
			RmqConnection = factory.CreateConnection();
			//BindEventHandlers(); // перенести в класс
		}




	}
}
