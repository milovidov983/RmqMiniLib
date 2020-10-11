using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal interface IReplyHandler {
		Task ReceiveReply(object model, BasicDeliverEventArgs ea);
	}
}
