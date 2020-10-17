using RabbitMQ.Client.Events;
using System;

namespace RmqLib {
	internal interface IConsumerManager {
		void BindEventHandlers(Action<AsyncEventingBasicConsumer> action);
		void InitConsumer();
		void RegisterUnsubscribeAction(Action<AsyncEventingBasicConsumer> action);
	}
}