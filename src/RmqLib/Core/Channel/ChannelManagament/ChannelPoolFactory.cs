using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	class ChannelPoolFactory : IChannelPoolFactory {
		private readonly IChannelEventsHandlerFactory channelEventsHandlerFactory;

		public ChannelPoolFactory(IChannelEventsHandlerFactory channelEventsHandlerFactory) {
			this.channelEventsHandlerFactory = channelEventsHandlerFactory;
		}

		public IChannelPool CreateChannelPool(IModel model) {
			var pool = new ChannelPool(model);

			var eventHandler = channelEventsHandlerFactory.CreateHandler();
			pool.BindEventHandlers(ch => {
				ch.ModelShutdown += eventHandler.ModelShutdown;
				ch.CallbackException += eventHandler.CallbackException;
				ch.BasicReturn += eventHandler.BasicReturn;
				ch.BasicRecoverOk += eventHandler.BasicRecoverOk;
				ch.BasicNacks += eventHandler.BasicNacks;
				ch.BasicAcks += eventHandler.BasicAcks;
				ch.FlowControl += eventHandler.FlowControl;
			});

			/// Будет вызвано в случае необходимости
			pool.RegisterUnsubscribeAction(ch => {
				if(eventHandler is null) {
					return;
				}
				ch.ModelShutdown -= eventHandler.ModelShutdown;
				ch.CallbackException -= eventHandler.CallbackException;
				ch.BasicReturn -= eventHandler.BasicReturn;
				ch.BasicRecoverOk -= eventHandler.BasicRecoverOk;
				ch.BasicNacks -= eventHandler.BasicNacks;
				ch.BasicAcks -= eventHandler.BasicAcks;
				ch.FlowControl -= eventHandler.FlowControl;
			});

			return pool;
		}


		public static IChannelPoolFactory Create(IChannelEventsHandlerFactory channelEventsHandlerFactory) {
			return new ChannelPoolFactory(channelEventsHandlerFactory);
		}
	}
}
