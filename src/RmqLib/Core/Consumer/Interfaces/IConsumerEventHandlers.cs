using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal interface IConsumerEventHandlers {
		void AddHandler(Func<object, ConsumerEventArgs, Task> consumerRegistredEventHandelr);
	}
}