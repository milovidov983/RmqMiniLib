using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	/// <summary>
	/// Отвечает за прием сообщений из шины
	/// </summary>
	internal class RequestHandler : IRequestHandler {
		/// <summary>
		/// Обработчик ошибок назначенный пользователем библиотеки
		/// </summary>
		private event ExceptionHandler exceptionEvent;

		private ICommands commands;
		private IModel channel;
		private readonly ILogger logger;
		private readonly string appId;

		internal RequestHandler(ILogger logger, string appId, ExceptionHandler consumerExceptionHandler = null) {
			this.logger = logger;
			this.appId = appId;
			if (consumerExceptionHandler != null) {
				this.exceptionEvent += consumerExceptionHandler;
			}
		}

		internal void Init(IModel commandChannel, ICommands commands) {
			this.channel = commandChannel;
			this.commands = commands;
		}

		/// <summary>
		/// Метод обрабатывает сообщения из шины и делегирует обработку классу отвечающему за конкретный топик
		/// </summary>
		internal async Task Handle(object _, BasicDeliverEventArgs ea) {
			var hasError = false;
			try {
				await ExecuteSpecificHandler(ea);
			} catch (Exception e) {
				hasError = await HandleException(ea, e);
			} finally {
				await NotifyRmq(ea, hasError);
			}
		}

		private async Task NotifyRmq(BasicDeliverEventArgs ea, bool hasError) {
			if (hasError) {
				await Task.Run(() =>
					channel.BasicReject(
						deliveryTag: ea.DeliveryTag,
						requeue: true)
				);

			} else {
				await Task.Run(() =>
					channel.BasicAck(
						deliveryTag: ea.DeliveryTag,
						multiple: false)
				);
			}
		}

		private async Task<bool> HandleException(BasicDeliverEventArgs ea, Exception e) {
			bool hasError = true;
			if (exceptionEvent != null) {
				try {
					await exceptionEvent(e, new DeliveredMessage(ea));
				} catch (Exception ex) {
					logger.LogError($"Ошибка в пользовательском обработчике исключений {ex.Message}");
				}
			} else {
				logger.LogError(e, $"Ошибка при обработке запроса \"{ea.RoutingKey}\", {e.Message}");
			}

			return hasError;
		}

		private async Task ExecuteSpecificHandler(BasicDeliverEventArgs ea) {
			var handler = commands.GetHandler(ea.RoutingKey);
			switch (handler) {
				case IRmqCommandHandler h:
					await HandleCommand(h, ea);
					break;
				case IRmqNotificationHandler h:
					await h.Execute(new DeliveredMessage(ea));
					break;
			}
		}

		private async Task HandleCommand(IRmqCommandHandler h, BasicDeliverEventArgs ea) {
			try {
				var res = await h.Execute(new DeliveredMessage(ea));
				await Reply(res.Result, null, ea.BasicProperties);
			} catch (RmqException ex) {
				await Reply(null, ex, ea.BasicProperties);
				throw;
			} catch (Exception ex) {
				await Reply(null, new RmqException(
						ex.Message,
						ex,
						Error.INTERNAL_ERROR),
					ea.BasicProperties);
				throw;
			}
		}

		private Task Reply(byte[] content, RmqException error, IBasicProperties basicProperties) {
			var replyProps = channel.CreateBasicProperties();
			replyProps.CorrelationId = basicProperties.CorrelationId;
			replyProps.ContentType = "text/json";
			if (error != null) {
				replyProps.Headers = new Dictionary<string, object>
				{
					{ "-x-error", error.Message },
					{ "-x-status-code", error.StatusCode },
					{ "-x-host", Dns.GetHostName() },
					{ "-x-service", appId },
					{ "-x-data", error.Data },
					//{ "-x-type", ErrorTypes.RMQ.ToString() },
				};
			} else {
				replyProps.Headers = new Dictionary<string, object>
				{
					{ "-x-service", appId },
					{ "-x-host", Dns.GetHostName() },
				};
			}

			return Task.Run(() => {
				channel.BasicPublish("", basicProperties.ReplyTo, replyProps, content);
			});
		}

		Task IRequestHandler.Handle(object _, BasicDeliverEventArgs ea) {
			throw new NotImplementedException();
		}
	}
}
