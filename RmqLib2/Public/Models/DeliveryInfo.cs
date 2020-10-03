using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib2 {
	public class DeliveryInfo {
        private IDictionary<string, object> headers;

		public DeliveryInfo(
			IDictionary<string, object> headers, 
			string exhangeName, 
			string topic, 
			string correlationId, 
			string replyTo) {

			this.headers = headers;
			ExhangeName = exhangeName;
			Topic = topic;
			CorrelationId = correlationId;
			ReplyTo = replyTo;
		}

		public string ExhangeName { get; private set; }

        public string Topic { get; private set; }

        public string CorrelationId { get; private set; }

        public string ReplyTo { get; private set; }
    }
}
