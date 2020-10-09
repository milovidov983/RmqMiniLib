using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class CreateChannelException : Exception {
		public CreateChannelException(string message) : base(message) {
		}
	}



	internal class ConnectionWrapperFactory : IConnectionWrapperFactory {
		private readonly RmqConfig config;

		private readonly ILogger logger;
		private IPublisherFactory PublisherFactory { get; }

		private readonly ConnectionWrapper connectionWrapper;


		private readonly IReplyHandler replyHandler;

		public ConnectionWrapperFactory(RmqConfig config, ILogger logger, IReplyHandler replyHandler) {
			this.config = config;
			this.logger = logger;
			this.replyHandler = replyHandler;


			connectionWrapper = new ConnectionWrapper(config);
			StartConnection();

			//PublisherFactory = new PublisherFactory(this, config.Exchange);
			//InitChannels().GetAwaiter().GetResult();
		}


		/// <summary>
		/// TODO comment
		/// </summary>
		private void StartConnection() {
			try {
				connectionWrapper.StartConnection().GetAwaiter().GetResult();
			} catch (Exception e) {
				var message = $"Failed connect to the RabbitMQ: {e.Message} ";
				logger?.LogError(message);
				throw;
			}
		}

		public async Task InitChannels() {
			var channelPool = await connectionWrapper.CreatechannelPool();
			if (channelPool.Status == ChannelCreatedStatus.Success) {
				var channel = channelPool.Create();
				var basicPublisher = PublisherFactory.GetBasicPublisher();
				BindReplyHandler(channel);
				await basicPublisher.InitChannel(channel);
			}
			throw new CreateChannelException(channelPool.Details);
		}

		



		public void Reconnect() {
			StartConnection();
			InitChannels().GetAwaiter().GetResult();
		}
	}
}
