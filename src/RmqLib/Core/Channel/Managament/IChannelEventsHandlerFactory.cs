using RabbitMQ.Client;

namespace RmqLib.Core {
	internal interface IChannelEventsHandlerFactory {
		IChannelEventsHandler CreateHandler(IChannelPool pool);
	}

	class ChannelEventsHandlerFactory : IChannelEventsHandlerFactory {
		public IChannelEventsHandler CreateHandler(IChannelPool pool) {
			return new ChannelEventsHandler(pool);
		}

		public static IChannelEventsHandlerFactory Create() {
			return new ChannelEventsHandlerFactory();
		}

	}
}