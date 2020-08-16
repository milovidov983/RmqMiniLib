using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {

	// TODO обернуть в свои модели что бы уши либы не торчали наружу

	/// <summary>
	/// Обработчики событий RMQ
	/// </summary>
	public class ConnectionEventHandlers {
		/// <summary>
		/// Raised when the connection is destroyed.
		/// </summary>
		public EventHandler<ShutdownEventArgs> ConnectionShutdown;
		/// <summary>
		/// Signalled when an exception occurs in a callback invoked by the connection.
		/// </summary>
		public EventHandler<CallbackExceptionEventArgs> CallbackException;

		/// <summary>
		/// 
		/// </summary>
		public EventHandler<ConnectionBlockedEventArgs> ConnectionBlocked;
		/// <summary>
		/// 
		/// </summary>
		public EventHandler<EventArgs> ConnectionUnblocked;
	}
}
