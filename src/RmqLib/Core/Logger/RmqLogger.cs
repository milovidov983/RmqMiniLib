using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal class RmqLogger : IRmqLogger {
		public RmqLogger() {
		}

		public void Debug(string message) {
			Console.WriteLine($"[{DateTime.Now.ToLocalTime()}][debug] {message}");
		}

		public void Error(string message) {
			Console.WriteLine($"[{DateTime.Now.ToLocalTime()}][error] {message}");
		}

		public void Info(string message) {
			Console.WriteLine($"[{DateTime.Now.ToLocalTime()}][info] {message}");
		}

		public void Warn(string message) {
			Console.WriteLine($"[{DateTime.Now.ToLocalTime()}][warning] {message}");
		}
	}

	internal class RmqLoggerPrefix : IRmqLogger {
		private readonly IRmqLogger rmqLogger;
		private readonly string prefix;



		public RmqLoggerPrefix(IRmqLogger rmqLogger, string prefix) {
			this.prefix = prefix;
			this.rmqLogger = rmqLogger;
		}


		public void Debug(string message) {
			rmqLogger.Debug($"[{prefix}] {message}");
		}

		public void Error(string message) {
			rmqLogger.Error($"[{prefix}] {message}");
		}

		public void Info(string message) {
			rmqLogger.Info($"[{prefix}] {message}");
		}

		public void Warn(string message) {
			rmqLogger.Warn($"[{prefix}] {message}");
		}
	}
}
