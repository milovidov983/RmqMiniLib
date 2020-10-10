using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RmqLib2.Core2 {
	internal class ConnectionManager : IConnectionManager {
		private readonly IChannelWrapper channel;

		public ConnectionManager(IChannelWrapper channel) {
			this.channel = channel;
		}

		public Task ConsumerRegistred(object sender, ConsumerEventArgs @event) {
			return channel.LockChannel();
		}


		public void ConnectionLostHandler(object sender, ShutdownEventArgs e) {
			channel.UnlockChannel();
		}
	}
}
