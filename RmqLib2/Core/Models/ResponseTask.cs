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
			Console.WriteLine("ResponseTask SetResult start");
			timer.Enabled = false;
			taskCompletionSource.SetResult(body);
			Console.WriteLine("ResponseTask SetResult end");
		}

		public Task<byte[]> GetResult() {
			Console.WriteLine("ResponseTask GetResult start");
			return taskCompletionSource.Task;
		}

		public void SetException(Exception exception) {
			timer.Enabled = false;
			taskCompletionSource.SetException(exception);
		}
	}
}
