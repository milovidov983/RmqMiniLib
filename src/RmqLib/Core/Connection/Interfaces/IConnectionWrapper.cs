using RabbitMQ.Client;
using System;

namespace RmqLib {
	internal interface IConnectionWrapper {
		IModel CreateChannel();
		bool IsOpen { get; }
		void BindEventHandler(Action<IConnection> config);
		void RegisterUnsubscribeHandler(Action<IConnection> config);
		void Abort();
	}
}
