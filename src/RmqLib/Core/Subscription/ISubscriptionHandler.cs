using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RmqLib {
	public interface ISubscriptionHandler {
		void Handle(object model, BasicDeliverEventArgs ea);
	}
}