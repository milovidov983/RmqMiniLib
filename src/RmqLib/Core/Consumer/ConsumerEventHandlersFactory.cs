using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal class ConsumerEventHandlersFactory : IConsumerEventHandlersFactory {
		private readonly IConsumerManager consumerManager;
		private readonly IRmqLogger logger;

		public ConsumerEventHandlersFactory(IConsumerManager consumerManager, IRmqLogger logger) {
			this.logger = logger;
			this.consumerManager = consumerManager;
		}

		public IConsumerEventHandlers CreateHandler() {
			return new ConsumerEventHandlers(consumerManager, logger);
		}

		public static IConsumerEventHandlersFactory Create(IRmqLogger logger, IConsumerManager consumerManager) {
			return new ConsumerEventHandlersFactory(consumerManager, logger);
		}
	}
}
