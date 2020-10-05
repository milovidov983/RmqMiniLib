namespace RmqLib2.Core2 {
	internal class RecconectionManager : IReconnectionManager {
		private IConnectionWrapper connection;
		private IChannelFactory channelFactory;

		public RecconectionManager(IConnectionWrapper connection, IChannelFactory channelFactory) {
			this.connection = connection;
			this.channelFactory = channelFactory;
		}
	}
}