using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.Threading;

namespace RmqLib {
	internal class ConnectionWrapper : IConnectionWrapper , IDisposable{
		private RabbitMQ.Client.IConnection connection;
		private Action<IConnection> unbindEventHandler;
		private readonly IConnectionFactory connectionFactory;
		private readonly Semaphore semaphore = new Semaphore(1,1);
		private readonly RmqConfig config;
		private readonly IRmqLogger logger;

		


		public bool IsOpen { get { return connection?.IsOpen ?? false; } }

		public ConnectionWrapper(RmqConfig config, IRmqLogger logger) {
			this.config = config;
			this.logger = logger;

			logger.Debug($"try to create {nameof(connectionFactory)}...");
			this.connectionFactory = InitConnectionFactory();

			logger.Debug($"try to create {nameof(connection)}...");
			connection = connectionFactory.CreateConnection();

			logger.Debug($"{nameof(connection)} created");

		}

		public void BindEventHandler(Action<IConnection> bindEventHandler) {
			bindEventHandler.Invoke(connection);
		}

		public void RegisterUnsubscribeHandler(Action<IConnection> unbindHandler) {
			this.unbindEventHandler = unbindHandler;
		}


		public IModel CreateChannel() {
			try {
				semaphore.WaitOne();
				logger.Debug($"{nameof(CreateChannel)} try to create channel...");

				var channel = connection.CreateModel();
				channel.ExchangeDeclare(config.Exchange, ExchangeType.Topic, durable: true);

				logger.Debug($"{nameof(CreateChannel)} channel created");
				return channel;
			} catch(Exception e) {
				logger.Error($"create channel error: {nameof(e.Message)}");
				throw;
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
					unbindEventHandler.Invoke(connection);
				} catch (Exception e) {
					logger.Error($"error to {nameof(unbindEventHandler)}: {nameof(e.Message)}");
				}
				connection.Close();
			}
		}
	}
}
