using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ConnectionManager : IConnectionManager {
		private IChannelWrapper rpcChannel;

		private IConnectionWrapper connection;

		private IChannelPoolFactory channelPoolFactory;
		private readonly IResponseMessageHandlerFactory responseMessageHandlerFactory;

		private IChannelPool rpcChannelPool;
		private IChannelPool subsChannelPool;
		private readonly RmqConfig config;

		public ConnectionManager(RmqConfig config, 
			IChannelPoolFactory channelPoolFactory,
			IResponseMessageHandlerFactory responseMessageHandlerFactory) {
			this.config = config;
			this.channelPoolFactory = channelPoolFactory;
			this.responseMessageHandlerFactory = responseMessageHandlerFactory;
			StartInitialization();
		}

		public void StartInitialization() {
			connection = new ConnectionWrapper(config);

			var rpcCh = connection.CreateChannel();
			var subsCh = connection.CreateChannel();

			rpcChannelPool = channelPoolFactory.CreateChannelPool(rpcCh);
			subsChannelPool = channelPoolFactory.CreateChannelPool(subsCh);

			rpcChannel = rpcChannelPool.GetChannel();

			InitEventHandlers();
			RegisterUnsubscribe();

			IResponseMessageHandler responseMessageHandler = responseMessageHandlerFactory.GetHandler();
			IConsumerBinder consumerBinder = new ConsumerBinder(rpcCh);
			IConsumerFactory consumerFactory = new ConsumerFactory(rpcCh);

			ConsumerManager consumerManager = new ConsumerManager(consumerFactory, responseMessageHandler, this, consumerBinder);
			consumerManager.InitConsumer();
		}

		public IChannelPool GetRpcChannelPool() {
			return rpcChannelPool;
		}
		public IChannelPool GetSubsChannelPool() {
			return subsChannelPool;
		}


		public IConnectionWrapper GetConnection() {
			return connection;
		}

		public void InitEventHandlers() {
			connection.BindEventHandlers((c) => {
				c.ConnectionBlocked += ConnectionBlocked;
				c.CallbackException += CallbackException;
				c.ConnectionUnblocked += ConnectionUnblocked;


			});
		}

		public void RegisterUnsubscribe() {
			connection.RegisterUnsubscribeAction((c) => {
				c.ConnectionBlocked -= ConnectionBlocked;
				c.CallbackException -= CallbackException;
				c.ConnectionUnblocked -= ConnectionUnblocked;


			});
		}



		private void ConnectionUnblocked(object sender, EventArgs e) {
			Console.WriteLine($"ConnectionEvent ConnectionUnblocked. ");
		}

		private void CallbackException(object sender, CallbackExceptionEventArgs e) {
			Console.WriteLine($"ConnectionEvent CallbackException. Exception.Message: {e.Exception.Message}");
		}

		private void ConnectionBlocked(object sender, ConnectionBlockedEventArgs e) {
			Console.WriteLine($"ConnectionEvent ConnectionBlocked. Reason: {e.Reason}");
		}
	}
}
