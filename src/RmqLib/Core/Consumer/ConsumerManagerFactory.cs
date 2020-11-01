using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal class ConsumerManagerFactory {
		private readonly IRmqLogger logger;

		public ConsumerManagerFactory(IRmqLogger logger) {
			this.logger = logger;
		}

		public IConsumerManager CreateConsumerManager(IConsumerFactory consumerFactory) {
			IConsumerManager manager = new ConsumerManager(consumerFactory, logger);
			manager.InitConsumer();

			return manager;
		}


		public static ConsumerManagerFactory Create(ConsumerType type) {
			ILoggerFactory loggerFactory = LoggerFactory.Create(type.ToString());
			var _logger = loggerFactory.CreateLogger(nameof(ConsumerManager));

			return new ConsumerManagerFactory(_logger);
		}
	}
}
