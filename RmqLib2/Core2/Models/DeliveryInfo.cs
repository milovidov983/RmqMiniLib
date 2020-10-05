using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib2 {
	public class DeliveryInfo : IDisposable {
		private readonly System.Timers.Timer timer;
		/// <summary>
		/// Ответ для текущего сообщения
		/// </summary>
		private readonly DeliveredMessage deliveredMessage;

		public string ExhangeName { get; private set; }
        public string Topic { get; private set; }
        public string CorrelationId { get; private set; }
		public byte[] Body { get; private set; }


		public DeliveryInfo(
			string exhangeName, 
			string topic, 
			byte[] body,
			DeliveredMessage deliveredMessage, 
			System.Timers.Timer timer) {

			ExhangeName = exhangeName;
			Topic = topic;
			CorrelationId = deliveredMessage.CorrelationId;
			Body = body;
			this.deliveredMessage = deliveredMessage;
			this.timer = timer;
		}
		


		public void StartTimer() {
			timer.Enabled = true;

			timer.Elapsed += (object source, ElapsedEventArgs e) => {
				deliveredMessage.SetElapsedTimeout(timer.Interval);

				timer.Dispose();
			}; ;
		}

		public void Dispose() {
			timer?.Dispose();
		}
	}
}
