using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal interface IPublisherFactory {
		IPublisher GetBasicPublisher();
	}

}
