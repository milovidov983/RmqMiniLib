using RabbitMQ.Client.Events;
using System;

namespace RmqLib.Core {
	internal interface IConsumerRegisterEventHandler {
		void AddHandler(Action<object, ConsumerEventArgs> handler);
		void RegisteredHandler(object sender, ConsumerEventArgs e);
	}
}