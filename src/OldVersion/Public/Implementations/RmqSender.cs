using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;
using System;
using System.Collections.Generic;
using System.Net;
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



		/// <summary>
		/// Ответить на полученную команду из rmq
		/// </summary>
		public Task SetRpcResultAsync(RequestContext request, ResponseMessage responseMessage) {
			var ea = request.GetBasicDeliverEventArgs();
			var replyProps = CreateReplyProps(ea);

			replyProps.Headers = new Dictionary<string, object>
			{
				{ "-x-service", rmqConfig.AppId },
				{ "-x-host", Dns.GetHostName() },
			};

			return Task.Run(() => {
				channel.Instance.BasicPublish("", ea.BasicProperties.ReplyTo, replyProps, responseMessage.Result);
			});
		}

		/// <summary>
		/// Отправить потребителю команды rmq, сообщение с ошибкой
		/// </summary>
		public Task SetRpcErrorAsync(RequestContext request, RmqException exception) {
			Validate(request, exception);

			var ea = request.GetBasicDeliverEventArgs();
			var replyProps = CreateReplyProps(ea);
			var additionalData = CreateAdditionalDataOrDefault(exception);

			replyProps.Headers = new Dictionary<string, object>
			{
				{ "-x-error", exception.Message },
				{ "-x-status-code", exception.StatusCode },
				{ "-x-host", Dns.GetHostName() },
				{ "-x-service", rmqConfig.AppId },
				{ "-x-data", additionalData }
			};

			return Task.Run(() => {
				channel.Instance.BasicPublish("", ea.BasicProperties.ReplyTo, replyProps, default);
			});
		}

		private static string CreateAdditionalDataOrDefault(RmqException exception) {
			if (exception.Data?.Count > 0) {
				return JsonSerializer.Serialize(exception.Data);
			}
			return string.Empty;
		}

		private void Validate(RequestContext request, RmqException exception) {
			if (request is null) {
				throw new ArgumentNullException($"Method {nameof(SetRpcErrorAsync)} parameter {nameof(request)}");
			}
			var (isValid, error) = request.IsValid();
			if (!isValid) {
				throw new ArgumentException($"Method {nameof(SetRpcErrorAsync)} parameter {nameof(request)}, {error}");
			}
			if (exception is null) {
				throw new ArgumentNullException($"Method {nameof(SetRpcErrorAsync)} parameter {nameof(exception)}");
			}
		}

		private IBasicProperties CreateReplyProps(BasicDeliverEventArgs ea) {
			var replyProps = channel.Instance.CreateBasicProperties();
			replyProps.CorrelationId = ea.BasicProperties.CorrelationId;
			replyProps.ContentType = "text/json";
			return replyProps;
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
