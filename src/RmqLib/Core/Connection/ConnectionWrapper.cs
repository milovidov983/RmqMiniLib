using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;

namespace RmqLib {
	internal class ConnectionWrapper : IConnectionWrapper , IDisposable{
		private RabbitMQ.Client.IConnection connection;
		private Action<IConnection> unsubscribeAction;
		private readonly IConnectionFactory connectionFactory;
		private readonly Semaphore semaphore = new Semaphore(1,1);
		private readonly RmqConfig config;


		public bool IsOpen { get { return connection?.IsOpen ?? false; } }

		public ConnectionWrapper(RmqConfig config) {
			this.config = config;
			this.connectionFactory = InitConnectionFactory();
			connection = connectionFactory.CreateConnection();

		}

		public void BindEventHandlers(Action<IConnection> config) {
			config.Invoke(connection);
		}

		public void UnBindEventHandlers(Action<IConnection> unsubscriptionAction) {
			this.unsubscribeAction = unsubscriptionAction;
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

		public void Dispose() {
			if(connection != null) {
				unsubscribeAction.Invoke(connection);
				connection.Close();
			}
		}
	}
}
