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
		public string AppId { get; set; }

		public int DeliveryAttemptCounter { get; set; } = 0;
		

		public DeliveryInfo(
			string exhangeName, 
			string topic, 
			byte[] body,
			string correlationId,
			string appId) {

			ExhangeName = exhangeName;
			Topic = topic;
			CorrelationId = correlationId;
			AppId = appId;
			Body = body;

		}
		

	}
}
