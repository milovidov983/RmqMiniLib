namespace RmqLib.Core {
	internal class ChannelEventsHandlerFactory : IChannelEventsHandlerFactory {
		private readonly IRmqLogger logger;

		public ChannelEventsHandlerFactory(IRmqLogger logger) {
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