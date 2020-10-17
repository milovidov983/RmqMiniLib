﻿using System;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib {
	/// <summary>
	/// Задача для ответа RPC вызова
	/// </summary>
	public class ResponseTask {
		private readonly TaskCompletionSource<byte[]> taskCompletionSource;
		/// <summary>
		/// Таймер ссылается на таймер в связанном deliveryInfo
		/// </summary>
		private readonly System.Timers.Timer timer;
		private readonly double interval;

		public ResponseTask(Timer timer) {
			this.taskCompletionSource = new TaskCompletionSource<byte[]>();
			this.timer = timer;
			this.interval = timer.Interval;
		}

		public void SetResult(byte[] body) {
			timer.Enabled = false;
			taskCompletionSource.SetResult(body);
		}

		public Task<byte[]> GetResult() {
			return taskCompletionSource.Task;
		}

		public void SetException(Exception exception) {
			timer.Enabled = false;
			taskCompletionSource.SetException(exception);
		}

		public void SetCanceled() {
			timer.Enabled = false;
			taskCompletionSource.SetException(new OperationCanceledException($"Task timeout canceled. Interval {interval}"));
		}
	}
}
