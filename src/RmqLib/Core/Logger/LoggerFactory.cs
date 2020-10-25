using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core.Logger {
	internal class LoggerFactory : ILoggerFactory {
		public IRmqLogger CreateLogger() {
			return new RmqLogger();
		}

		public static ILoggerFactory Create() {
			return new LoggerFactory();
		}
	}
}
