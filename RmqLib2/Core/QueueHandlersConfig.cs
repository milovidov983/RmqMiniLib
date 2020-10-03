using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib2 {
	internal class QueueHandlersConfig : IQueueHandlersConfig {
		public Func<DeliveredMessage, MessageProcessResult, MessageProcessResult> afterExecuteHandler;
		public Func<DeliveredMessage, bool> beforeExecuteHandler;
		public Func<Exception, DeliveredMessage, bool> onExceptionHandler;

		public readonly Dictionary<string, IRabbitCommand> commandHandlers = new Dictionary<string, IRabbitCommand>();

		private IRabbitHub rabbitHub;

		public QueueHandlersConfig(IRabbitHub rabbitHub) {
			this.rabbitHub = rabbitHub;
		}

		public IQueueHandlersConfig AfterExecute(Func<DeliveredMessage, MessageProcessResult, MessageProcessResult> handler) {
			afterExecuteHandler = handler;
			return this;
		}

		public IQueueHandlersConfig BeforeExecute(Func<DeliveredMessage, bool> handler) {
			beforeExecuteHandler = handler;
			return this;
		}

		public IQueueHandlersConfig OnException(Func<Exception, DeliveredMessage, bool> handler) {
			onExceptionHandler = handler;
			return this;
		}

		public IQueueHandlersConfig OnTopic(string topic, IRabbitCommand command) {
			command.WithHub(rabbitHub);
			commandHandlers.Add(topic, command);
			return this;
		}
	}
}
