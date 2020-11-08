using RmqLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	/// <summary>
	/// Инициализация обработчиков сообщений и middleware
	/// </summary>
	public class QueueHandlersConfig : IQueueHandlersConfig {
		/// <summary>
		/// Коллекция пользовательских обработчиков команд для топиков rabbitMq
		/// </summary>
		internal readonly Dictionary<string, IRabbitCommand> commandHandlers = new Dictionary<string, IRabbitCommand>();
		internal Func<DeliveredMessage, MessageProcessResult, Task<MessageProcessResult>> AfterExecuteHandler { get; set; }
		internal Func<DeliveredMessage, Task<bool>> BeforeExecuteHandler { get; set; }
		internal Func<Exception, DeliveredMessage, Task<bool>> OnExceptionHandler { get; set; }
		internal Func<DeliveredMessage, Task<MessageProcessResult>> OnUnexpectedTopicHandler { get; set; }


		private readonly IRabbitHub rabbitHub;
		public QueueHandlersConfig(IRabbitHub rabbitHub) {
			this.rabbitHub = rabbitHub;
		}

		/// <summary>
		/// Middleware вызывается всегда и после того как клиентский обработчик отработал
		/// </summary>
		public IQueueHandlersConfig AfterExecute(Func<DeliveredMessage, MessageProcessResult, Task<MessageProcessResult>> handler) {
			AfterExecuteHandler = handler;
			return this;
		}

		/// <summary>
		/// Middleware вызывается всегда до того как клиентский обработчик начал обработку
		/// </summary>
		public IQueueHandlersConfig BeforeExecute(Func<DeliveredMessage, Task<bool>> handler) {
			BeforeExecuteHandler = handler;
			return this;
		}

		/// <summary>
		/// Middleware вызывается при возникновении исключения при выполнении клиентского обработчика
		/// </summary>
		public IQueueHandlersConfig OnException(Func<Exception, DeliveredMessage, Task<bool>> handler) {
			OnExceptionHandler = handler;
			return this;
		}

		/// <summary>
		/// Middleware вызывается если приходит сообщение с Topic для которого не зарегистрирован обработчик
		/// </summary>
		public IQueueHandlersConfig OnUnexpectedTopic(Func<DeliveredMessage, Task<MessageProcessResult>> handler) {
			OnUnexpectedTopicHandler = handler;
			return this;
		}

		/// <summary>
		/// Зарегистрировать обработчик для topic
		/// </summary>
		public IQueueHandlersConfig OnTopic(string topic, IRabbitCommand command) {
			command.WithHub(rabbitHub);
			commandHandlers.Add(topic, command);
			return this;
		}

	}
}
