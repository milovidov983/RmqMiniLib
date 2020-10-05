using System;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib2 {
	/// <summary>
	/// Задача для ответа RPC вызова
	/// </summary>
	public class ResponseTask {
		private readonly TaskCompletionSource<byte[]> taskCompletionSource;
		/// <summary>
		/// Таймер ссылается на таймер в связанном deliveryInfo
		/// </summary>
		private readonly System.Timers.Timer timer;

		public ResponseTask(Timer timer) {
			this.taskCompletionSource = new TaskCompletionSource<byte[]>();
			this.timer = timer;
		}

		public void SetResult(byte[] body) {
			timer?.Stop();
			taskCompletionSource.SetResult(body);
		}

		public async Task<byte[]> GetResult() {
			return await taskCompletionSource.Task;
		}

		public void SetException(Exception exception) {
			taskCompletionSource.SetException(exception);
			timer?.Stop();
		}
	}
}
