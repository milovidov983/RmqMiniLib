using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RmqLib2 {
	public class ResponseMessage {
		private byte[] responseBody;

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
			TResponse response = Deserialize<TResponse>(responseBody);
			return response;

		}

		public async Task WaitResult() {
			responseBody = await ResponseTask.GetResult();
		}

		private TResponse Deserialize<TResponse>(byte[] responseBody) where TResponse : class {
			var json = System.Text.Encoding.UTF8.GetString(responseBody);
			if (!string.IsNullOrWhiteSpace(json)) {
				return Deserialize<TResponse>(json);
			}
			return default;
		}
		private static TResponse Deserialize<TResponse>(string res) where TResponse : class {
			try {
				return JsonSerializer.Deserialize<TResponse>(res);
			} catch (Exception exteption) {
				var exceptionMessage
					= $"Deserialize to type \"{typeof(TResponse).FullName}\" error: {exteption.Message}";
				throw new Exception(
					exceptionMessage,
					exteption);
			}
		}

		public void SetElapsedTimeout() {
			ResponseTask.SetCanceled();
		}

		public bool HasError { get => Headers?.ContainsKey("-x-error") == true; }

		public string GetError() {
			if (HasError) {
				return (string)Headers["-x-error"];
			}
			return null;
		}
	}
}
