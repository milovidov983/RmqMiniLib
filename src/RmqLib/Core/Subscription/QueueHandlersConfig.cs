using RmqLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	/// <summary>
	/// Concept TODO доделать
	/// </summary>
	internal class QueueHandlersConfig : IQueueHandlersConfig {


		public readonly Dictionary<string, IRabbitCommand> commandHandlers = new Dictionary<string, IRabbitCommand>();

		private IRabbitHub rabbitHub;

		public QueueHandlersConfig(IRabbitHub rabbitHub) {
			this.rabbitHub = rabbitHub;
		}

		public IQueueHandlersConfig AfterExecute(Func<ResponseMessage, MessageProcessResult, Task<MessageProcessResult>> handler) {
			//afterExecuteHandler = handler;
			return this;
		}

		public IQueueHandlersConfig BeforeExecute(Func<ResponseMessage, Task<bool>> handler) {
			///beforeExecuteHandler = handler;
			return this;
		}

		public IQueueHandlersConfig OnException(Func<Exception, ResponseMessage, Task<bool>> handler) {
			///onExceptionHandler = handler;
			return this;
		}

		public IQueueHandlersConfig OnUnexpectedTopic(Func<ResponseMessage, Task<MessageProcessResult>> handler) {
			///onUnexpectedTopicHandler = handler;
			return this;
		}

		public IQueueHandlersConfig OnTopic(string topic, IRabbitCommand command) {
			command.WithHub(rabbitHub);
			commandHandlers.Add(topic, command);
			return this;
		}
	}
}
