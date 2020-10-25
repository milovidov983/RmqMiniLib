using RabbitMQ.Client;
using System;

namespace RmqLib.Core {
	internal interface IConnectionEventHandlers {
		void AddHandler(Action<object, ShutdownEventArgs> connectionShutdownEventHandler);
	}
}