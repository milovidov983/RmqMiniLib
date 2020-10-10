using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib2 {
	public class DeliveryInfo {
		public string ExhangeName { get; private set; }
        public string Topic { get; private set; }
        public string CorrelationId { get; private set; }
		public byte[] Body { get; private set; }

		

		public DeliveryInfo(
			string exhangeName, 
			string topic, 
			byte[] body,
			string correlationId) {

			ExhangeName = exhangeName;
			Topic = topic;
			CorrelationId = correlationId;
			Body = body;

		}
		

	}
}
