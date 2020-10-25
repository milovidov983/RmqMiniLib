namespace RmqLib.Core {
	internal class ConnectionEventsHandlerFactory : IConnectionEventsHandlerFactory {
		private readonly IConnectionWrapper connectionWrapper;
		private readonly IRmqLogger logger;

		public ConnectionEventsHandlerFactory(IConnectionWrapper connectionWrapper, IRmqLogger logger)  {
			this.logger = logger;
			this.connectionWrapper = connectionWrapper;
		}

		public IConnectionEventHandlers CreateHandler() {
			return new ConnectionEventHandlers(connectionWrapper, logger);
		}

		public static IConnectionEventsHandlerFactory Create(IRmqLogger logger, IConnectionWrapper connectionWrapper) {
			return new ConnectionEventsHandlerFactory(connectionWrapper, logger);
		}

	}
}