using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal interface IReplyHandler {
		void AddReplySubscription(string correlationId, DeliveredMessage resonseHandler);
		Task ReceiveReply(object model, BasicDeliverEventArgs ea);
		DeliveredMessage RemoveReplySubscription(string correlationId);
	}
}
