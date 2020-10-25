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


		internal readonly Dictionary<string, IRabbitCommand> commandHandlers = new Dictionary<string, IRabbitCommand>();

		internal Func<DeliveredMessage, MessageProcessResult, Task<MessageProcessResult>> AfterExecuteHandler { get; private set; }
		internal Func<DeliveredMessage, Task<bool>> BeforeExecuteHandler { get; private set; }
		internal Func<Exception, DeliveredMessage, Task<bool>> OnExceptionHandler { get; private set; }
		internal Func<DeliveredMessage, Task<MessageProcessResult>> OnUnexpectedTopicHandler { get; private set; }


		private readonly IRabbitHub rabbitHub;


		public QueueHandlersConfig(IRabbitHub rabbitHub) {
			this.rabbitHub = rabbitHub;
		}

		public IQueueHandlersConfig AfterExecute(Func<DeliveredMessage, MessageProcessResult, Task<MessageProcessResult>> handler) {
			AfterExecuteHandler = handler;
			return this;
		}

		public IQueueHandlersConfig BeforeExecute(Func<DeliveredMessage, Task<bool>> handler) {
			BeforeExecuteHandler = handler;
			return this;
		}

		public IQueueHandlersConfig OnException(Func<Exception, DeliveredMessage, Task<bool>> handler) {
			OnExceptionHandler = handler;
			return this;
		}

		public IQueueHandlersConfig OnUnexpectedTopic(Func<DeliveredMessage, Task<MessageProcessResult>> handler) {
			OnUnexpectedTopicHandler = handler;
			return this;
		}

		public IQueueHandlersConfig OnTopic(string topic, IRabbitCommand command) {
			command.WithHub(rabbitHub);
			commandHandlers.Add(topic, command);
			return this;
		}

	}
}
