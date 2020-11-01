using Microsoft.Extensions.Logging;
using RmqLib.Core.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal class LoggerFactory : ILoggerFactory {
		private readonly string prefix;
		private static ILogger externalLogger;

		public LoggerFactory(string prefix) {
			this.prefix = prefix;
		}

		public static void SetupExternalLogger(ILogger externalLogger) {
			LoggerFactory.externalLogger = externalLogger;
		}

		public IRmqLogger CreateLogger() {
			if (externalLogger is null) {
				return new RmqLogger();
			}
			return new ExternalLogger(externalLogger);
		}

		public IRmqLogger CreateLogger(string moduleName) {
			var logMessage = string.IsNullOrEmpty(prefix)
				? $"{moduleName}"
				: $"{moduleName}:{prefix}";

			if (externalLogger is null) {
				return new RmqLoggerPrefix(new RmqLogger(), logMessage);
			}
			return new RmqLoggerPrefix(new ExternalLogger(externalLogger), logMessage);
		}


		public static ILoggerFactory Create(string prefix) {
			return new LoggerFactory(prefix);
		}
	}
}
