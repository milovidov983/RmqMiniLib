using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RmqLib {
	public class ResponseMessage {
		private ReadOnlyMemory<byte> responseBody;

		public string RoutingKey { get; set; }

        public IDictionary<string, object> Headers { get; set; }

        public string CorrelationId { get; set; }

        public string ReplyTo { get; set; }

        public string AppId { get; set; }

		public ResponseTask ResponseTask { get; set; }

		public ResponseMessage(ResponseTask responseTask, string correlationId) {
			CorrelationId = correlationId;
			ResponseTask = responseTask;
		}

		public TResponse GetResponse<TResponse>() where TResponse : class {
			TResponse response = JsonSerializer.Deserialize<TResponse>(responseBody.Span, JsonOptions.Default);
			return response;

		}

		public async Task WaitResult() {
			responseBody = await ResponseTask.GetResult();
		}


		public void SetElapsedTimeout() {
			ResponseTask.SetCanceled();
		}

		public bool HasError { get => Headers?.ContainsKey(RmqLib.Core.Headers.Error) == true; }

		public string GetError() {
			if (HasError) {
				return Encoding.UTF8.GetString((byte[])Headers[RmqLib.Core.Headers.Error]);
			}
			return null;
		}
	}
}
