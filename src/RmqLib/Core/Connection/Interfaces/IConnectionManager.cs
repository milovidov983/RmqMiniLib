using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RmqLib {
	internal interface IConnectionManager {
		Task ConsumerRegistred(object sender, ConsumerEventArgs @event);
		IConnectionWrapper GetConnection();
	}
}
