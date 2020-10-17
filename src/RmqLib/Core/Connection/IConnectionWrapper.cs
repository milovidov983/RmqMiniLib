using RabbitMQ.Client;
using System;

namespace RmqLib {
	internal interface IConnectionWrapper {
		IModel CreateChannel();
		bool IsOpen { get; }
		void BindEventHandlers(Action<IConnection> config);
		void RegisterUnsubscribeAction(Action<IConnection> config);
	}
}
