using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core.Logger {
	class ExternalLogger : IRmqLogger {
		ILogger logger;

		public ExternalLogger(ILogger logger) {
			this.logger = logger;
		}

		public void Debug(string message) {
			logger.LogDebug(message);
		}

		public void Error(string message) {
			logger.LogError(message);	
		}

		public void Info(string message) {
			logger.LogInformation(message);
		}

		public void Warn(string message) {
			logger.LogWarning(message);
		}
	}
}
