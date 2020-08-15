using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {
	/// <summary>
	/// Обработчики событий RMQ
	/// </summary>
	public class OtherEventHandlers {

		// Не реализовано

		/// <summary>
		/// TODO на будущее пользовательский обработчик перед выполнением команды
		/// </summary>
		public EventHandler<DeliveredMessage> BeforeExecuteHandler;		
		
		/// <summary>
		/// TODO на будущее пользовательский обработчик после выполнения команды
		/// </summary>
		public EventHandler<DeliveredMessage> AfterExecuteHandler;

		/// <summary>
		/// TODO на будущее пользовательский обработчик после выполнения команды
		/// </summary>
		public Func<Exception, DeliveredMessage> ExceptionHandler;
	}
}
