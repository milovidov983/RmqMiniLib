using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {
	/// <summary>
	/// Обработчики событий RMQ
	/// </summary>
	public class EventHandlers {
		/// <summary>
		/// Raised when the connection is destroyed.
		/// </summary>
		public EventHandler<ShutdownEventArgs> ConnectionShutdown;
		/// <summary>
		/// Signalled when an exception occurs in a callback invoked by the connection.
		/// </summary>
		public EventHandler<CallbackExceptionEventArgs> CallbackException;

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
