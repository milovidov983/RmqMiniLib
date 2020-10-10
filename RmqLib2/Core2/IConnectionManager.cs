using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RmqLib2.Core2 {
	internal interface IConnectionManager {
		void ConnectionLostHandler(object sender, ShutdownEventArgs e);
		Task ConsumerRegistred(object sender, ConsumerEventArgs @event);
	}
}
