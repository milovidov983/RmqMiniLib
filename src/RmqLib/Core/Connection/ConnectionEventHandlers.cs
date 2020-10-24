using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core.Connection {
	class ConnectionEventHandlers {
		private readonly List<Action<IConnection>> activeEventHandlers = new List<Action<IConnection>>();
		private readonly List<Action<IConnection>> unsubscribeEventHandlers = new List<Action<IConnection>>();

		public void AddEventHandler(Action<ICollection> initAction) {

		}


 	}
}
