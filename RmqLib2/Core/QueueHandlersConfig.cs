using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib2 {
	/// <summary>
	/// Concept TODO доделать
	/// </summary>
	internal class QueueHandlersConfig : IQueueHandlersConfig {
		public Func<ResponseMessage, MessageProcessResult, MessageProcessResult> afterExecuteHandler;
		public Func<ResponseMessage, bool> beforeExecuteHandler;
		public Func<Exception, ResponseMessage, bool> onExceptionHandler;

		public readonly Dictionary<string, IRabbitCommand> commandHandlers = new Dictionary<string, IRabbitCommand>();

		private IRabbitHub rabbitHub;

		public QueueHandlersConfig(IRabbitHub rabbitHub) {
			this.rabbitHub = rabbitHub;
		}

		public IQueueHandlersConfig AfterExecute(Func<ResponseMessage, MessageProcessResult, MessageProcessResult> handler) {
			afterExecuteHandler = handler;
			return this;
		}

		public IQueueHandlersConfig BeforeExecute(Func<ResponseMessage, bool> handler) {
			beforeExecuteHandler = handler;
			return this;
		}

		public IQueueHandlersConfig OnException(Func<Exception, ResponseMessage, bool> handler) {
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
