using RmqLib.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace RmqLib {
	public class RmqSender : IRmqSender {
		private readonly IChannel channel;
		private readonly RmqConfig rmqConfig;
		private readonly IResponseHandelr responseHandler;

		internal RmqSender(
			RmqConfig rmqConfig,
			IResponseHandelr responseHandler,
			IChannel channel) {
			this.responseHandler = responseHandler;
			this.channel = channel;
			this.rmqConfig = rmqConfig;
		}

		/// <summary>
		/// Выполнить RPC запрос к сервису через RMQ
		/// </summary>
		public async Task<TResponse> Send<TRequest, TResponse>(string topic, TRequest message, TimeSpan? timeout = null)
			where TRequest : class
			where TResponse : class {

			var correlationId = Guid.NewGuid().ToString();
			var completionTask = new TaskCompletionSource<string>();
			responseHandler.AddReplySubscription(correlationId, completionTask);

			var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message ?? new object()));
			await channel.SendRpc(topic, messageBytes, correlationId);
			var timer = CreateTimer(timeout, correlationId);

			try {
				return await GetRusult<TResponse>(completionTask, timer);
			} catch (OperationCanceledException) {
				throw;
			} catch (RmqException) {
				throw;
			} catch (Exception e) {
				// TODO потестировать данный случай и по результатам изменить exceptionMessage
				var exceptionMessage = $"RMQ unhandled exception: {e.Message}";
				throw new Exception(exceptionMessage, completionTask.Task.Exception);

			} finally {
				timer.Dispose();
				responseHandler.RemoveReplySubscription(correlationId);
			}
		}

		/// <summary>
		/// Отправить сообщение оповещение
		/// </summary>
		public async Task Notify<TMessage>(string topic, TMessage message) where TMessage : class {
			if (string.IsNullOrWhiteSpace(topic)) {
				throw new ArgumentNullException($"RMQ error, parameter \"{nameof(topic)}\" is null or empty!");
			}
			var messageBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message ?? new object()));
			await channel.SendNotify(topic, messageBytes);
		}

		private Timer CreateTimer(TimeSpan? timeout, string correlationId) {
			timeout = timeout ?? rmqConfig.RequestTimeout;
			var timer = new Timer(timeout.Value.TotalMilliseconds) {
				Enabled = true
			};

			timer.Elapsed += (object source, ElapsedEventArgs e) => {
				var complTask = responseHandler.RemoveReplySubscription(correlationId);
				complTask.SetException(new OperationCanceledException($"RMQ request timeout after {timeout.Value.TotalSeconds} sec"));
				timer.Dispose();
			};
			return timer;
		}

		private static async Task<TResponse> GetRusult<TResponse>(TaskCompletionSource<string> completionTask, Timer timer) 
			where TResponse : class {

			var res = await completionTask.Task;
			timer.Stop();
			if (!string.IsNullOrWhiteSpace(res)) {
				return GetResult<TResponse>(res);
			}
			return default;
		}

		private static TResponse GetResult<TResponse>(string res) where TResponse : class {
			try {
				return JsonSerializer.Deserialize<TResponse>(res);
			} catch (Exception exteption) {
				var exceptionMessage
					= $"Deserialize to type \"{typeof(TResponse).FullName}\" error: {exteption.Message}";
				throw new RmqException(
					exceptionMessage,
					exteption,
					Error.INTERNAL_ERROR);
			}
		}
	}
}
