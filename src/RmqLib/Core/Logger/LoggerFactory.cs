using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal class LoggerFactory : ILoggerFactory {
		private readonly string prefix;

		public LoggerFactory(string prefix) {
			this.prefix = prefix;
		}

		public IRmqLogger CreateLogger() {
			return new RmqLogger();
		}


		public IRmqLogger CreateLogger(string moduleName) {
			return new RmqLoggerPrefix(new RmqLogger(), $"[{moduleName}:{prefix}]" );
		}


		public static ILoggerFactory Create(string prefix) {
			return new LoggerFactory(prefix);
		}
	}
}
