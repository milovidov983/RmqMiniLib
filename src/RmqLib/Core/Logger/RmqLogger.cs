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
}
