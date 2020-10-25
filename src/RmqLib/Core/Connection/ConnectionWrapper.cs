using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.Threading;

namespace RmqLib {
	internal class ConnectionWrapper : IConnectionWrapper , IDisposable{
		private RabbitMQ.Client.IConnection connection;
		private Action<IConnection> unsubscribeAction;
		private readonly IConnectionFactory connectionFactory;
		private readonly Semaphore semaphore = new Semaphore(1,1);
		private readonly RmqConfig config;
		private readonly IRmqLogger logger;

		


		public bool IsOpen { get { return connection?.IsOpen ?? false; } }

		public ConnectionWrapper(RmqConfig config, IRmqLogger logger) {
			this.config = config;
			this.logger = logger;
			this.connectionFactory = InitConnectionFactory();
			connection = connectionFactory.CreateConnection();

		}

		public void SetSettings(Action<IConnection> config) {
			try {
				config.Invoke(connection);
			} catch(Exception e) {
				logger.Error($"{nameof(ConnectionWrapper)} error in {nameof(SetSettings)}: {nameof(e.Message)}");
			}
		}

		public void RegisterUnsubscribeAction(Action<IConnection> action) {
			this.unsubscribeAction = action;
		}


		public IModel CreateChannel() {
			try {
				semaphore.WaitOne();
				logger.Debug($"{nameof(ConnectionWrapper)} {nameof(CreateChannel)} try to create channel...");

				var channel = connection.CreateModel();
				channel.ExchangeDeclare(config.Exchange, ExchangeType.Topic, durable: true);

				logger.Debug($"{nameof(ConnectionWrapper)} {nameof(CreateChannel)} channel created");
				return channel;
			} catch(Exception e) {
				logger.Error($"{nameof(ConnectionWrapper)} try to create channel {nameof(SetSettings)}: {nameof(e.Message)}");
				return null;
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
				try {
					unsubscribeAction.Invoke(connection);
				} catch (Exception e) {
					logger.Error($"{nameof(ConnectionWrapper)} error in {nameof(Dispose)}: {nameof(e.Message)}");
				}
				connection.Close();
			}
		}
	}
}
