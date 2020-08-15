using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core.Implementations {
	public class RequestHandler {
		/// <summary>
		/// Обработчик ошибок назначенный пользователем библиотеки
		/// </summary>
		private event ExceptionHandler consumerExceptionHandler;
		private readonly IRmqLoggerService logger;
		private readonly IHandlersManager handlersManager;
		private readonly IModel channel;
		private readonly string appId;

		public MainHandler(IHandlersManager handlersManager, IModel commandChannel, string appId) {
			this.logger = logger;
			this.handlersManager = handlersManager;
			this.channel = commandChannel;
			this.appId = appId;
		}

		public async Task Handle(object _, BasicDeliverEventArgs ea) {
			var hasError = false;
			try {
				var handler = handlersManager.GetHandler(ea.RoutingKey);
				switch (handler) {
					case IRmqCommandHandler h:
						await HandleCommand(h, ea);
						break;
					case IRmqNotificationHandler h:
						await h.Execute(new DeliveredMessage(ea));
						break;
				}

			} catch (Exception e) {
				hasError = true;
				if (consumerExceptionHandler != null) {
					try {
						await consumerExceptionHandler(e, new DeliveredMessage(ea));
					} catch (Exception ex) {
						logger.Error($"Ошибка в пользовательском обработчике исключений {ex.Message}");
					}
				}
			} finally {
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
		}

		private async Task HandleCommand(IRmqCommandHandler h, BasicDeliverEventArgs ea) {
			try {
				var res = await h.Execute(new DeliveredMessage(ea));
				await Reply(res.Result, null, ea.BasicProperties);
			} catch (RmqException error) {
				await Reply(null, error, ea.BasicProperties);
				throw;
			} catch (Exception error) {
				await Reply(null, new RmqException(
						error.Message,
						error,
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
					{ "-x-type", ErrorTypes.RMQ.ToString() },
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
	}
}
