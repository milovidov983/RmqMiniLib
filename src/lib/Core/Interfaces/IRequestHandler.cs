using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	public interface IRequestHandler {
		Task Handle(object _, BasicDeliverEventArgs ea);
	}
}
