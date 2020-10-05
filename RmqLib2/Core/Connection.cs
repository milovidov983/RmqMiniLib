using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class CreateChannelException : Exception {
		public CreateChannelException(string message) : base(message) {
		}
	}
	internal enum ChannelCreatedStatus {
		Success = 1,
		ConnectionIsClose,
		UnknownError
	}

	internal class RmqChannelFactory {
		RabbitMQ.Client.IModel channel;

		public string Details { get; }
		public ChannelCreatedStatus Status { get; }

		public RabbitMQ.Client.IModel Create() {
			return channel;
		}

		public RmqChannelFactory(ChannelCreatedStatus status, RabbitMQ.Client.IModel channel, string details = null) {
			this.Status = status;
			this.channel = channel;
			this.Details = details;

		}


	}


	internal class ConnectionManager {
		private RabbitMQ.Client.IConnection connection;
		private readonly IConnectionFactory factory;
		private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);

		public ConnectionManager(IConnectionFactory factory) {
			this.factory = factory;
		}

		public async Task<RmqChannelFactory> CreateChannelFactory() {
			try {
				await semaphore.WaitAsync();
				return CreateFactory();
			} catch (Exception ex) {
				var errorStatus = ChannelCreatedStatus.UnknownError;
				return new RmqChannelFactory(errorStatus, null, ex.Message);
			} finally {
				semaphore.Release();
			}
		}

		private RmqChannelFactory CreateFactory() {
			if (connection.IsOpen) {
				var channel = connection.CreateModel();
				var succesStatus = ChannelCreatedStatus.Success;
				return new RmqChannelFactory(succesStatus, channel);
			} else {
				var errorStatus = ChannelCreatedStatus.ConnectionIsClose;
				return new RmqChannelFactory(errorStatus, null, errorStatus.ToString());
			}
		}

		public async Task CreateRmqConnection() {
			try {
				await semaphore.WaitAsync();
				connection = factory.CreateConnection();
			} finally {
				semaphore.Release();
			}
		}

	}


	internal class Connection : IConnectionManager {
		private readonly RmqConfig config;

		private readonly ILogger logger;
		private IPublisherFactory PublisherFactory { get; }

		private readonly ConnectionManager connectionManager;

		private readonly IConnectionFactory factory;

		private readonly IReplyHandler replyHandler;

		public Connection(RmqConfig config, ILogger logger) {
			this.config = config;
			this.logger = logger;
			factory = InitConnectionFactory();
			// TODO сделать зависимость явной
			replyHandler = new ReplyHandelr();
			

			connectionManager = new ConnectionManager(factory);

			StartConnection();
			PublisherFactory = new PublisherFactory(connectionManager, config.Exchange);
			InitChannels().GetAwaiter().GetResult();
		}


		/// <summary>
		/// TODO comment
		/// </summary>
		private void StartConnection() {
			try {
				connectionManager.CreateRmqConnection().GetAwaiter().GetResult();
			} catch (Exception e) {
				var message = $"Failed connect to the RabbitMQ: {e.Message} ";
				logger?.LogError(message);
				throw;
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


		private async Task InitChannels() {
			var channelFactory = await connectionManager.CreateChannelFactory();
			if (channelFactory.Status == ChannelCreatedStatus.Success) {
				var channel = channelFactory.Create();
				var basicPublisher = PublisherFactory.GetBasicPublisher();
				BindReplyHandler(channel, replyHandler);
				await basicPublisher.InitChanel(channel);
			}
			throw new CreateChannelException(channelFactory.Details);
		}

		/// <summary>
		/// TODO comment
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="handler"></param>
		private void BindReplyHandler(IModel channel, IReplyHandler handler) {
			var consumer = new AsyncEventingBasicConsumer(channel);
			channel.BasicConsume(
				consumer: consumer,
				queue: ServiceConstants.REPLY_QUEUE_NAME,
				autoAck: true);
			consumer.Received += handler.ReceiveReply;
		}

		public void Reconnect() {
			StartConnection();
			InitChannels().GetAwaiter().GetResult();
		}
	}
}
