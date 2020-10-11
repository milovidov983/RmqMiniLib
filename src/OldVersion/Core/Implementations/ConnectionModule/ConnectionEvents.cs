using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	/// <summary>
	/// Отвечает за привязку событий к сущности "соединение с rmq"
	/// </summary>
	internal class ConnectionEvents : IConnectionEvents {
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly ConnectionEventHandlers connectionEventHandlers;

		public ConnectionEvents(IEventHandlersFactoryService eventFactory) {
			this.connectionEventHandlers = eventFactory?.CreateConnectionEventHandlers();
		}


		/// <summary>
		/// 
		/// </summary>
		public void BindEventHandlers(IConnectionService connectionService) {
			if (connectionEventHandlers?.ConnectionShutdown != null) {
				connectionService.RmqConnection.ConnectionShutdown += connectionEventHandlers.ConnectionShutdown;
			}
			if (connectionEventHandlers?.CallbackException != null) {
				connectionService.RmqConnection.CallbackException += connectionEventHandlers.CallbackException;
			}
			if (connectionEventHandlers?.ConnectionBlocked != null) {
				connectionService.RmqConnection.ConnectionBlocked += connectionEventHandlers.ConnectionBlocked;
			}
			if (connectionEventHandlers?.ConnectionUnblocked != null) {
				connectionService.RmqConnection.ConnectionUnblocked += connectionEventHandlers.ConnectionUnblocked;
			}
		}
	}
}
