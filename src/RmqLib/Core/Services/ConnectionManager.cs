using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ConnectionManager : IConnectionManager {
		private readonly IResponseMessageHandlerFactory responseMessageHandlerFactory;
		private readonly IRmqLogger logger;
		private readonly RmqConfig config;

		private IConnectionWrapper connection;
		private IChannelPoolFactory channelPoolFactory;
		private IChannelPool rpcChannelPool;
		private IResponseMessageHandler responseMessageHandler;
		private ChannelGuardService channelGuardService;
		private IModel subsCh;

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
			var connectionLoggerFactory = LoggerFactory.Create("");
			var connectionLogger = connectionLoggerFactory.CreateLogger(nameof(ConnectionWrapper));
			connection = new ConnectionWrapper(config, connectionLogger);

			var rpcCh = connection.CreateChannel();


			rpcChannelPool = channelPoolFactory.CreateChannelPool(rpcCh);

			IConsumerBinder rpcConsumerBinder = new RpcConsumerBinder();
			IConsumerFactory rpcConsumerFactory = new ConsumerFactory(rpcCh, rpcConsumerBinder);
			
			var loggerFactory = LoggerFactory.Create(ConsumerType.Rpc.ToString());
			var managerLogger = loggerFactory.CreateLogger(nameof(ConsumerManager));

			IConsumerManager rpcConsumerManager = new ConsumerManager(
				rpcConsumerFactory, 
				managerLogger);

			rpcConsumerManager.InitConsumer();

			IMainConsumerEventHandlerFactory rpcMainEventHandlerFactory 
				= ConsumerEventHandlersFactory.Create(rpcConsumerManager, loggerFactory);

			var rpcConsumerMainEventHandler = rpcMainEventHandlerFactory.CreateMainHandler();



			var connectionHandlerLogger = loggerFactory.CreateLogger(nameof(ConnectionEventHandlers));

			IConnectionEventsHandlerFactory connectionEventsHandlerFactory 
				= ConnectionEventsHandlerFactory.Create(connectionHandlerLogger, connection);
			IConnectionEventHandlers connectionEventHandler = connectionEventsHandlerFactory.CreateHandler();


			var channelGuardLogger = loggerFactory.CreateLogger(nameof(ChannelGuardService));
			this.channelGuardService 
				= new ChannelGuardService(
					rpcChannelPool, // <--- TODO только rpc?
					logger, 
					connectionEventHandler, 
					rpcConsumerMainEventHandler);

			// Подписка на ответы запросов rpc
			responseMessageHandler = responseMessageHandlerFactory.GetHandler();
			rpcConsumerMainEventHandler.AddReceiveHandler(responseMessageHandler.HandleMessage);

		}

		public IResponseMessageHandler GetResponseMessageHandler() {
			return responseMessageHandler;
		}

		public IChannelPool GetRpcChannelPool() {
			return rpcChannelPool;
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


			return channelPoolFactory.CreateChannelPool(subsCh);
		}

		public IModel GetSubscriptionChannel() {
			return subsCh;
		}


		public IConnectionWrapper GetConnection() {
			return connection;
		}

	}
}
