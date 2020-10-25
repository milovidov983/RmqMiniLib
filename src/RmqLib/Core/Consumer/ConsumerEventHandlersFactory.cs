using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal class ConsumerEventHandlersFactory : IMainConsumerEventHandlerFactory, IConsumerEventHandlersFactory {
		private readonly IConsumerManager consumerManager;
		private readonly ILoggerFactory loggerFactory;

		public ConsumerEventHandlersFactory(
			IConsumerManager consumerManager, 
			ILoggerFactory loggerFactory) {

			this.consumerManager = consumerManager;
			this.loggerFactory = loggerFactory;
		}


		/// <summary>
		/// Создать главный обработчик событий для consumer
		/// </summary>
		public IConsumerMainEventHandlers CreateMainHandler() {
			var logger = loggerFactory.CreateLogger(nameof(ConsumerMainEventHandlers));
			return new ConsumerMainEventHandlers(this, consumerManager, logger);
		}

		/// <summary>
		/// Создать обработчик события при регистрации consumer
		/// </summary>
		public IConsumerRegisterEventHandler CreateRegisterEventHandler() {
			var logger = loggerFactory.CreateLogger(nameof(ConsumerRegisterEventHandler));
			return new ConsumerRegisterEventHandler(logger);
		}

		/// <summary>
		/// Создать обработчик события который обрабатывает получение сообщения
		/// </summary>
		public IConsumerReceiveEventHandelr CreateReceiveEventHandelr() {
			var logger = loggerFactory.CreateLogger(nameof(ConsumerReceiveEventHandelr));
			return new ConsumerReceiveEventHandelr(logger);
		}


		/// <summary>
		/// Создать фабрику обработчиков события consumer
		/// </summary>
		public static IMainConsumerEventHandlerFactory Create(
			IConsumerManager consumerManager,
			ILoggerFactory loggerFactory) {

			return new ConsumerEventHandlersFactory(consumerManager, loggerFactory);
		}
	}
}
