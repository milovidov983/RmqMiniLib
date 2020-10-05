using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal interface IPublisherFactory {
		IPublisher GetBasicPublisher();
	}

	internal interface IBasicPublisher {
		void Publish(DeliveryInfo deliveryInfo, Payload payload);

	}

	internal interface IPublisher {
		//Func<DeliveredMessage, Task<MessageProcessResult>> OnMessage { get; set; }

		Task InitChanel(IModel chanel);
		DeliveredMessage Publish(DeliveryInfo deliveryMessage);
	}



	internal class PublishRequest {
		public PublishRequest(DeliveryInfo deliveryInfo, Payload payload) {
			DeliveryInfo = deliveryInfo;
			Payload = payload;
		}

		public DeliveryInfo DeliveryInfo { get;}
		public Payload Payload { get; }
	}
}
