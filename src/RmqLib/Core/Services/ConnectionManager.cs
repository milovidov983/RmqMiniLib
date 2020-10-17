using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ConnectionManager : IConnectionManager {
		private IChannelWrapper rpcChannel;
		private IChannelWrapper subscriptionChannel;

		private IConnectionWrapper connection;

		private IChannelPoolFactory channelPoolFactory;

		private readonly RmqConfig config;

		public ConnectionManager(RmqConfig config, IChannelPoolFactory channelPoolFactory) {
			this.config = config;
			this.channelPoolFactory = channelPoolFactory;
			StartInitialization();
		}

		public void StartInitialization() {
			connection = new ConnectionWrapper(config);

			var rpcCh = connection.CreateChannel();
			var subsCh = connection.CreateChannel();

			var rpcChannelPool = channelPoolFactory.CreateChannelPool(rpcCh);
			var subsChannelPool = channelPoolFactory.CreateChannelPool(subsCh);

			rpcChannel = rpcChannelPool.GetChannel();
			subscriptionChannel = subsChannelPool.GetChannel();

			InitEventHandlers();
		}




		public IConnectionWrapper GetConnection() {
			return connection;
		}

		public void InitEventHandlers() {
			connection.BindEventHandlers((c) => {
				c.ConnectionBlocked += ConnectionBlocked;
				c.CallbackException += CallbackException;
				c.ConnectionUnblocked += ConnectionUnblocked;
				c.ConnectionShutdown += ConnectionShutdown;
				c.ConnectionShutdown += ConnectionLostHandler;
			});
		}




		public Task ConsumerRegistred(object sender, ConsumerEventArgs @event) {
			Console.WriteLine($"ConsumerRegistred.");
			rpcChannel.UnlockChannel();
			return Task.CompletedTask;
		}
		public void ConnectionLostHandler(object sender, ShutdownEventArgs e) {
			Console.WriteLine($"ConnectionLostHandler. {e.Cause}");
			rpcChannel.LockChannel();
		}


		private void ConnectionShutdown(object sender, ShutdownEventArgs e) {
			Console.WriteLine($"ConnectionShutdown. {e.Cause}");
		}

		private void ConnectionUnblocked(object sender, EventArgs e) {
			Console.WriteLine($"ConnectionUnblocked. ");
		}

		private void CallbackException(object sender, CallbackExceptionEventArgs e) {
			Console.WriteLine($"CallbackException. {e.Exception.Message}");
		}

		private void ConnectionBlocked(object sender, ConnectionBlockedEventArgs e) {
			Console.WriteLine($"ConnectionBlocked. {e.Reason}");
		}
	}
}
