using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal class RmqLogger : IRmqLogger {
		public void Debug(string message) {
			Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}][debug] {message}");
		}

		public void Error(string message) {
			Console.WriteLine($"[{DateTime.Now.ToShortTimeString()}][error] {message}");
		}
	}
}
