using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {
	public class DeliveredMessage {
		public DeliveredMessage(
			IBasicProperties replyProps, 
			string routingKey, 
			ReadOnlyMemory<byte> body,
			ulong deliveryTag) {


			ReplyProps = replyProps; // channel.CreateBasicProperties();
			RoutingKey = routingKey; //ea.RoutingKey;
			Body = body; //ea.Body.ToArray();
			DeliveryTag = deliveryTag;// ea.DeliveryTag;
		}


		// topic
		public string RoutingKey { get; private set; }


		public ulong DeliveryTag { get; private set; }
		public IBasicProperties ReplyProps { get; private set; }

		public ReadOnlyMemory<byte> Body { get; private  set; }
		
	}
}
