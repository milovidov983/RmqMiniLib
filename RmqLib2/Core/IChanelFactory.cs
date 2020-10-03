using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;

namespace RmqLib2 {
	internal interface IChanelFactory {
		IRmqChanel GetOutChannel();
	}

	internal interface IRmqChanel {
		//Func<DeliveredMessage, Task<MessageProcessResult>> OnMessage { get; set; }

		void Send(DeliveryInfo deliveryInfo, Payload payload);
		void InitChanel(IModel chanel);
	}



	internal struct Request {
		public Request(DeliveryInfo deliveryInfo, Payload payload) {
			DeliveryInfo = deliveryInfo;
			Payload = payload;
		}

		public DeliveryInfo DeliveryInfo { get;}
		public Payload Payload { get; }
	}
}
