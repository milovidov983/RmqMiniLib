using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib.Core {
	public class DeliveryInfo {
		public static string AppId { get; set; }
		public static string ExhangeName { get; set; }

        public string Topic { get; private set; }
        public string CorrelationId { get; private set; }
		public byte[] Body { get; private set; }
		public string ReplyTo { get; set; }
		

		public DeliveryInfo(
			string topic, 
			byte[] body,
			string correlationId,
			string replyTo ){ 


			Topic = topic;
			CorrelationId = correlationId;
			ReplyTo = replyTo;
			Body = body;
		}
		

	}
}
