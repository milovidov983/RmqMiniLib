using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Public.Models {
	public class DeliveredMessage {
		public string AppId { get; set; }
		public string RoutingKey { get; set; }
		public string ReplyTo { get; set; }
		public string CorrelationId { get; set; }
		public byte[] Body { get; set; }
	}
}
