using RabbitMQ.Client;
using RmqLib2.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class ConnectionWrapper : IConnectionWrapper {
		private RabbitMQ.Client.IConnection connection;
		private readonly IConnectionFactory connectionFactory;
		private readonly Semaphore semaphore = new Semaphore(1,1);
		private readonly RmqConfig config;


		public bool IsOpen { get { return connection?.IsOpen ?? false; } }

		public ConnectionWrapper(RmqConfig config) {
			this.config = config;
			this.connectionFactory = InitConnectionFactory();
			StartConnection();

		}



		public IModel CreateChannel() {
			try {
				semaphore.WaitOne();
				var channel = connection.CreateModel();
				channel.ExchangeDeclare(config.Exchange, ExchangeType.Topic, durable: true);
				return channel;

			} finally {
				semaphore.Release();
			}
		}

		public void StartConnection() {
			try {
				semaphore.WaitOne();
				connection = connectionFactory.CreateConnection();
			} finally {
				semaphore.Release();
			}
		}

		public void AddConnectionShutdownHandler(IConnectionManager connectionManager) {
			connection.ConnectionShutdown += connectionManager.ConnectionLostHandler;
		}


		private IConnectionFactory InitConnectionFactory() {
			var factory = new RabbitMQ.Client.ConnectionFactory {
				HostName = config.HostName,
				Password = config.Password,
				UserName = config.UserName
			};
			factory.DispatchConsumersAsync = true;
			factory.AutomaticRecoveryEnabled = true;
			return factory;
		}

	}
}
