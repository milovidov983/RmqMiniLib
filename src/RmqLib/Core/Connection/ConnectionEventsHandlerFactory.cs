namespace RmqLib.Core {
	internal class ConnectionEventsHandlerFactory : IConnectionEventsHandlerFactory {
		private readonly IRmqLogger logger;

		public ConnectionEventsHandlerFactory(IRmqLogger logger) {
			this.logger = logger;
		}

		public IChannelEventsHandler CreateHandler() {
			return new ChannelEventsHandler(logger);
		}

		public static IChannelEventsHandlerFactory Create(IRmqLogger logger) {
			return new ChannelEventsHandlerFactory(logger);
		}

	}
}