﻿using RabbitMQ.Client;
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
		private IChannelPool subsChannelPool;
		private IResponseMessageHandler responseMessageHandler;
		private ChannelGuardService channelGuardService;

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

			

			IConsumerFactory consumerFactory = new ConsumerFactory(rpcCh);
			IConsumerBinder consumerBinder = new ConsumerBinder(rpcCh);

			IConsumerManager consumerManager = new ConsumerManager(consumerFactory, consumerBinder, logger);
			consumerManager.InitConsumer();



			IConsumerEventHandlersFactory consumerEventHandlersFactory 
				= ConsumerEventHandlersFactory.Create(logger, consumerManager);
			var consumerEventHandlers = consumerEventHandlersFactory.CreateHandler();


			IConnectionEventsHandlerFactory connectionEventsHandlerFactory 
				= ConnectionEventsHandlerFactory.Create(logger, connection);
			var connectionEventHandler = connectionEventsHandlerFactory.CreateHandler();



			this.channelGuardService 
				= new ChannelGuardService(
					rpcChannelPool, // <--- TODO только rpc?
					logger, 
					connectionEventHandler, 
					consumerEventHandlers);

			// Подписка на ответы запросов rpc
			responseMessageHandler = responseMessageHandlerFactory.GetHandler();
			consumerEventHandlers.AddHandler(responseMessageHandler.HandleMessage);

		}

		public IResponseMessageHandler GetResponseMessageHandler() {
			return responseMessageHandler;
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
