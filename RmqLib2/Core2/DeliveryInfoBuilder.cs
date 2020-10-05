using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class DeliveryInfoBuilder : IDeliveryInfoBuilder {
		private TimeSpan defaultTimout = new TimeSpan(0, 0, 20);
		private string exchangeName = "default_exchange";

		public DeliveryInfoBuilder(TimeSpan defaultTimout, string exchangeName) {
			this.defaultTimout = defaultTimout;
			this.exchangeName = exchangeName;
		}

		public DeliveryInfo CreateDeliveryInfo<TRequest>(string topic, TRequest request, TimeSpan? timeout)
			where TRequest : class {

			timeout = timeout ?? defaultTimout;

			System.Timers.Timer timer = new System.Timers.Timer(timeout.Value.Milliseconds);
			ResponseTask responseTask = new ResponseTask(timer);

			DeliveredMessage dm = new DeliveredMessage(responseTask);

			byte[] body = Serialize(request);
			DeliveryInfo di = new DeliveryInfo(exchangeName, topic, body, dm, timer);

			return di;

		}

		private byte[] Serialize<TRequest>(TRequest request) where TRequest : class {
			return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request ?? new object()));
		}
	}
}
