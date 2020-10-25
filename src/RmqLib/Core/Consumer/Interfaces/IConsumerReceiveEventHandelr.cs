using RabbitMQ.Client.Events;
using System;

namespace RmqLib.Core {
	internal interface IConsumerReceiveEventHandelr {
		void AddHandler(Action<object, BasicDeliverEventArgs> handler);
		void ReceiveHandler(object sender, BasicDeliverEventArgs e);
	}
}