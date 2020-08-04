using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {
	/// <summary>
	/// Обработчики событий соединения с RMQ
	/// </summary>
	public class ConnectionEventHandlers {
		/// <summary>
		/// Raised when the connection is destroyed.
		/// </summary>
		public EventHandler<ShutdownEventArgs> ConnectionShutdown;
		/// <summary>
		///  Signalled when an exception occurs in a callback invoked by the connection.
		/// </summary>
		public EventHandler<CallbackExceptionEventArgs> CallbackException;
	}
}
