using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal interface IConsumerCommonEventHandelr {
		Task ShutdownHandler(object sender, ShutdownEventArgs @event);
		Task UnregisteredHandler(object sender, ConsumerEventArgs @event);
		Task ConsumerCancelledHandler(object sender, ConsumerEventArgs @event);
	}
}