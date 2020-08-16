using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ConsumerExceptionHandler : IConsumerExceptionHandler {
		private readonly ILogger logger;

		/// <summary>
		/// Обработчик ошибок назначенный пользователем библиотеки
		/// </summary>
		private event ExceptionHandler exceptionEvent;


		public ConsumerExceptionHandler(ILogger logger, ExceptionHandler consumerExceptionHandler) {
			this.logger = logger;
			if (consumerExceptionHandler != null) {
				this.exceptionEvent += consumerExceptionHandler;
			}
		}

		public async Task HandleException(DeliveredMessage dm, Exception serviceException) {
			var msg = $"Ошибка при обработке запроса \"{dm.Topic}\", {serviceException.Message}";
			if (exceptionEvent != null) {
				try {
					await exceptionEvent(serviceException, dm);
				} catch (Exception ex) {
					logger.LogError(serviceException, $"Ошибка в пользовательском обработчике исключений {ex.Message}. Исходное исключение: {msg}");
				}
			} else {
				logger.LogError(serviceException, msg);
			}
		}
	}
}
