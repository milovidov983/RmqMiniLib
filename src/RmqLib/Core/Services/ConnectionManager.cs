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
		private readonly IRmqLogger logger;
		private IChannelPool rpcChannelPool;
		private IChannelPool subsChannelPool;
		private ChannelGuardService channelGuardService;
		private readonly RmqConfig config;

		public ConnectionManager(RmqConfig config, 
			IChannelPoolFactory channelPoolFactory,
			IResponseMessageHandlerFactory responseMessageHandlerFactory,
			IRmqLogger logger) {
			this.config = config;
			this.channelPoolFactory = channelPoolFactory;
			this.responseMessageHandlerFactory = responseMessageHandlerFactory;
			this.logger = logger;
			StartInitialization();
		}

		public void StartInitialization() {
			connection = new ConnectionWrapper(config, logger);

			var rpcCh = connection.CreateChannel();
			var subsCh = connection.CreateChannel();

			rpcChannelPool = channelPoolFactory.CreateChannelPool(rpcCh);
			subsChannelPool = channelPoolFactory.CreateChannelPool(subsCh);

			rpcChannel = rpcChannelPool.GetChannel();

			IConsumerFactory consumerFactory = new ConsumerFactory(rpcCh);
			IConsumerBinder consumerBinder = new ConsumerBinder(rpcCh);

			IConsumerManager consumerManager = new ConsumerManager(consumerFactory, consumerBinder, logger);
			consumerManager.InitConsumer();



			IConsumerEventHandlersFactory consumerEventHandlersFactory 
				= ConsumerEventHandlersFactory.Create(logger, consumerManager);
			IConnectionEventsHandlerFactory connectionEventsHandlerFactory 
				= ConnectionEventsHandlerFactory.Create(logger, connection);

			var consumerEventHandlers = consumerEventHandlersFactory.CreateHandler();
			var connectionEventHandler = connectionEventsHandlerFactory.CreateHandler();



			this.channelGuardService 
				= new ChannelGuardService(
					rpcChannelPool, // <--- TODO только rpc?
					logger, 
					connectionEventHandler, 
					consumerEventHandlers);

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

	}
}
