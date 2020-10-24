using RabbitMQ.Client;

namespace RmqLib.Core {
	internal interface IChannelEventsHandlerFactory {
		IChannelEventsHandler CreateHandler();
	}

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