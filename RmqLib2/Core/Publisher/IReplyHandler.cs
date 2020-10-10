using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal interface IReplyHandler {
		void AddReplySubscription(string correlationId, ResponseMessage resonseHandler);
		Task ReceiveReply(object model, BasicDeliverEventArgs ea);
		ResponseMessage RemoveReplySubscription(string correlationId);
	}
}
