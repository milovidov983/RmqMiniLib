using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RmqLib {
	public interface ISubscriptionManager {
		void Handler(object model, BasicDeliverEventArgs ea);
	}
}