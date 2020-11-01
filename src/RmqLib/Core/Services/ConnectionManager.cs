using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ConnectionManager : IConnectionManager {
		private readonly IResponseMessageHandlerFactory responseMessageHandlerFactory;
		private readonly RmqConfig config;

		private IConnectionWrapper connection;
		private IChannelPoolFactory channelPoolFactory;
		private IChannelPool rpcChannelPool;
		private IResponseMessageHandler responseMessageHandler;
		private ChannelGuardService channelGuardService;
		private IModel subsCh;
		private IChannelWrapper subscriptionWrapChannel;

		public ConnectionManager(RmqConfig config, 
			IChannelPoolFactory channelPoolFactory,
			IResponseMessageHandlerFactory responseMessageHandlerFactory) {
			this.config = config;
			this.channelPoolFactory = channelPoolFactory;
			this.responseMessageHandlerFactory = responseMessageHandlerFactory;
			StartInitialization();
		}

		public void StartInitialization() {
			ILoggerFactory connectionLoggerFactory = LoggerFactory.Create("");
			IRmqLogger connectionLogger = connectionLoggerFactory.CreateLogger(nameof(ConnectionWrapper));
			connection = new ConnectionWrapper(config, connectionLogger);

			IModel rpcCh = connection.CreateChannel();


			rpcChannelPool = channelPoolFactory.CreateChannelPool(rpcCh);

			IConsumerBinder rpcConsumerBinder = new RpcConsumerBinder();
			IConsumerFactory rpcConsumerFactory = new ConsumerFactory(
				rpcChannelPool.GetChannelWrapper(), 
				rpcConsumerBinder);

			ILoggerFactory loggerFactory = LoggerFactory.Create(ConsumerType.Rpc.ToString());
			IRmqLogger managerLogger = loggerFactory.CreateLogger(nameof(ConsumerManager));

			IConsumerManager rpcConsumerManager = new ConsumerManager(
				rpcConsumerFactory, 
				managerLogger);
			rpcConsumerManager.InitConsumer();


			IMainConsumerEventHandlerFactory rpcMainEventHandlerFactory 
				= ConsumerEventHandlersFactory.Create(rpcConsumerManager, loggerFactory);

			IConsumerMainEventHandlers rpcConsumerMainEventHandler 
				= rpcMainEventHandlerFactory.CreateMainHandler();



			IRmqLogger connectionHandlerLogger = loggerFactory.CreateLogger(nameof(ConnectionEventHandlers));
			IConnectionEventsHandlerFactory connectionEventsHandlerFactory 
							= ConnectionEventsHandlerFactory.Create(connectionHandlerLogger, connection);
			IConnectionEventHandlers connectionEventHandler = connectionEventsHandlerFactory.CreateHandler();


			IRmqLogger channelGuardLogger = loggerFactory.CreateLogger(nameof(ChannelGuardService));
			this.channelGuardService 
				= new ChannelGuardService(
					rpcChannelPool, // <--- TODO только rpc?
					channelGuardLogger, 
					connectionEventHandler, 
					rpcConsumerMainEventHandler);

			// Подписка на ответы запросов rpc
			responseMessageHandler = responseMessageHandlerFactory.GetHandler();
			rpcConsumerMainEventHandler.AddReceiveHandler(responseMessageHandler.HandleMessage);

		}

		public IResponseMessageHandler GetResponseMessageHandler() {
			return responseMessageHandler;
		}

		public IChannelWrapper GetRpcChannel() {
			return rpcChannelPool.GetChannelWrapper();
		}

		public IChannelPool CreateSubscriptionChannelPool(ushort prefechCount) {
			if (subsCh is null) {
				subsCh = connection.CreateChannel();
				subsCh.BasicQos(0, prefechCount, false);
				subsCh.QueueDeclare(
					config.Queue,
					durable: true,
					exclusive: false,
					autoDelete: false);
			}


			IChannelPool pool = channelPoolFactory.CreateChannelPool(subsCh);
			subscriptionWrapChannel = pool.GetChannelWrapper();
			return pool;
		}

		public IChannelWrapper GetSubscriptionChannel() {
			return subscriptionWrapChannel;
		}


		public IConnectionWrapper GetConnection() {
			return connection;
		}
	}
}
