using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace RmqLib {
	public class DeliveredMessage {
		public DeliveredMessage(
			IBasicProperties replyProps, 
			string routingKey, 
			ReadOnlyMemory<byte> body,
			ulong deliveryTag) {


			ReplyProps = replyProps; 
			RoutingKey = routingKey; 
			Body = body; 
			DeliveryTag = deliveryTag;
		}



		public string RoutingKey { get; private set; }


		public ulong DeliveryTag { get; private set; }
		public IBasicProperties ReplyProps { get; private set; }

		public ReadOnlyMemory<byte> Body { get; private  set; }



		public bool IsRpcMessage() {
			if (Guid.TryParse(ReplyProps?.CorrelationId ?? "", out var _)) {
				return !string.IsNullOrEmpty(ReplyProps?.ReplyTo);
			}
			return false;
		}

		public TResponse GetContent<TResponse>() where TResponse : class {
			TResponse response = JsonSerializer.Deserialize<TResponse>(Body.Span);
			return response;

		}

		public string GetTopic() {
			return RoutingKey;
		}


		public string GetAppId() {
			return ReplyProps.AppId;
		}

	}
}
