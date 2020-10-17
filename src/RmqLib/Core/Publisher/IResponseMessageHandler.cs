using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace RmqLib {
	internal interface IResponseMessageHandler {
		void AddSubscription(string correlationId, ResponseMessage resonseHandler);
		Task HandleMessage(object model, BasicDeliverEventArgs ea);
		ResponseMessage RemoveSubscription(string correlationId);
	}
}
