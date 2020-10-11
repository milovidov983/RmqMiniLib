using RabbitMQ.Client.Events;
using RmqLib.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RmqLib {
	public class RequestContext {
		/// <summary>
		/// Объект с служебной информацией и телом сообщения. Относится к текущему вызову
		/// </summary>
		private readonly BasicDeliverEventArgs basicDeliverEventArgs;
		/// <summary>
		/// Топик rmq для которого создано сообщение
		/// </summary>
		public string Topic { get => basicDeliverEventArgs.RoutingKey; }
		/// <summary>
		/// AppId потребителя
		/// </summary>
		public string AppId { get => basicDeliverEventArgs?.BasicProperties?.AppId; }
		/// <summary>
		/// Экземпляр объекта позволяющего работать с шиной
		/// </summary>
		public IRmqSender Hub { get; }

		public RequestContext(BasicDeliverEventArgs basicDeliverEventArgs, IRmqSender hub) {
			this.basicDeliverEventArgs = basicDeliverEventArgs;
			Hub = hub;
		}


		public TResult GetContent<TResult>(bool validation = true) where TResult : class {
			var content = basicDeliverEventArgs.GetContent<TResult>();

			if (validation) {
				ValidateContent(content);
			}
			return content;
		}

		private static void ValidateContent<TResult>(TResult content) where TResult : class {
			var validationResults = new List<ValidationResult>();
			var context = new ValidationContext(content);
			var isValidationFailed = !Validator.TryValidateObject(content, context, validationResults, true);
			if (isValidationFailed) {
				var validationErrorDetails = string.Join(", ", validationResults.Select(x => x.ErrorMessage));
				var exceptionMessage = $"Rmq validation error: {validationErrorDetails}";

				throw new RmqException(exceptionMessage,Error.INVALID_REQUEST_CODE);
			}
		}

		
		public BasicDeliverEventArgs GetBasicDeliverEventArgs() {
			return basicDeliverEventArgs;
		}


		public bool IsRpcMessage() {
			return !string.IsNullOrEmpty(basicDeliverEventArgs.BasicProperties?.ReplyTo);
		}
	}
}