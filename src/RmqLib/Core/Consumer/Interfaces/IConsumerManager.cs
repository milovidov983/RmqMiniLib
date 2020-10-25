using RabbitMQ.Client;
using System;

namespace RmqLib {
	internal interface IConsumerManager {
		void BindEventHandlers(Action<IAsyncBasicConsumer> action);
		void InitConsumer();
		void RegisterUnsubscribeAction(Action<IAsyncBasicConsumer> action);
	}
}