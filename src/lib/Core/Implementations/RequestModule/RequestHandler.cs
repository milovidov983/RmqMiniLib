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
	/// Передает сообщения из rmq в обработчик
	/// </summary>
	internal class RequestHandler : IRequestHandler {
		private readonly ICommandHandlersManager commands;
		private readonly IModel channel;
		private readonly string appId;
		private readonly IRmqSender hub;

		internal RequestHandler(
			string appId,
			IChannel commandChannel,
			ICommandHandlersManager commands, 
			IRmqSender hub) {
			this.appId = appId;
			this.channel = commandChannel.Instance;
			this.commands = commands;
			this.hub = hub;
		}


		/// <summary>
		/// Метод обрабатывает сообщения из шины и делегирует обработку классу отвечающему за конкретный топик
		/// </summary>
		public async Task Handle(object _, BasicDeliverEventArgs ea) {
			MessageProcessResult messageProcessResult = MessageProcessResult.Reject;
			try {
				var handler = commands.GetHandler(ea.RoutingKey);
				var deliveredMessage = new RequestContext(ea, hub);

				messageProcessResult = await handler.Execute(deliveredMessage);
			} finally {
				await AskRmq(messageProcessResult, ea);
			}
		}

		private Task AskRmq(MessageProcessResult processResult, BasicDeliverEventArgs ea) {
			switch (processResult) {
				case MessageProcessResult.Ack:
					return Task.Run(() =>
					channel.BasicAck(
						deliveryTag: ea.DeliveryTag,
						multiple: false)
					);
				case MessageProcessResult.Requeue:
					return Task.Run(() =>
					channel.BasicReject(
						deliveryTag: ea.DeliveryTag,
						requeue: true)
					);
				case MessageProcessResult.Reject:
					return Task.Run(() =>
					channel.BasicReject(
						deliveryTag: ea.DeliveryTag,
						requeue: false)
					);
			}
			throw new ApplicationException($"MessageProcessResult is invalid: {processResult}");
		}



		/// <summary>
		/// Ответить на полученную команду из rmq
		/// </summary>
		public Task SetRpcResultAsync(RequestContext request, ResponseMessage responseMessage) {
			var ea = request.GetBasicDeliverEventArgs();
			var replyProps = CreateReplyProps(ea);

			replyProps.Headers = new Dictionary<string, object>
			{
				{ "-x-service", appId },
				{ "-x-host", Dns.GetHostName() },
			};
			
			return Task.Run(() => {
				channel.BasicPublish("", ea.BasicProperties.ReplyTo, replyProps, responseMessage.Result);
			});
		}

		/// <summary>
		/// Отправить потребителю команды rmq, сообщение с ошибкой
		/// </summary>
		public Task SetRpcErrorAsync(RequestContext request, string error, int? statusCode) {
			var ea = request.GetBasicDeliverEventArgs();
			var replyProps = CreateReplyProps(ea);

			replyProps.Headers = new Dictionary<string, object>
			{
				{ "-x-error", error },
				{ "-x-status-code", statusCode },
				{ "-x-host", Dns.GetHostName() },
				{ "-x-service", appId }
			};

			return Task.Run(() => {
				channel.BasicPublish("", ea.BasicProperties.ReplyTo, replyProps, default);
			});
		}

		private IBasicProperties CreateReplyProps(BasicDeliverEventArgs ea) {
			var replyProps = channel.CreateBasicProperties();
			replyProps.CorrelationId = ea.BasicProperties.CorrelationId;
			replyProps.ContentType = "text/json";
			return replyProps;
		}
	}
}
