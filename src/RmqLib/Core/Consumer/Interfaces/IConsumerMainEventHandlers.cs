using RabbitMQ.Client.Events;
using System;

namespace RmqLib.Core {
	internal interface IConsumerMainEventHandlers {
		void AddRegisterHandler(Action<object, ConsumerEventArgs> handler);
		void AddReceiveHandler(Action<object, BasicDeliverEventArgs> handler);
	}
}