using RabbitMQ.Client.Events;
using RmqLib.Core;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RmqLib {
	public class DeliveredMessage {
		private readonly BasicDeliverEventArgs basicDeliverEventArgs;
		public string Topic { get => basicDeliverEventArgs.RoutingKey; }

		public DeliveredMessage(BasicDeliverEventArgs basicDeliverEventArgs) {
			this.basicDeliverEventArgs = basicDeliverEventArgs;
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
	}
}
