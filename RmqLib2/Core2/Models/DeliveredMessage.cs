using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib2 {
	public class DeliveredMessage {
		public string RoutingKey { get; set; }

        public IDictionary<string, object> Headers { get; set; }

        public string CorrelationId { get; set; }

        public string ReplyTo { get; set; }

        public string AppId { get; set; }

		public ResponseTask ResponseTask { get; set; }

		public DeliveredMessage(ResponseTask responseTask) {
			CorrelationId = Guid.NewGuid().ToString("N");
			ResponseTask = responseTask;
		}

		public async Task<TResponse> GetResponse<TResponse>() where TResponse : class {
			var responseBody = await ResponseTask.GetResult();
			TResponse response = Deserialize<TResponse>(responseBody);
			return response;

		}

		private TResponse Deserialize<TResponse>(byte[] responseBody) where TResponse : class {
			throw new NotImplementedException();
		}


		public void SetElapsedTimeout(double totalMilliseconds) {
			ResponseTask.SetException(
				new OperationCanceledException($"RMQ request timeout after {totalMilliseconds} milliseconds"));
		}
	}
}
