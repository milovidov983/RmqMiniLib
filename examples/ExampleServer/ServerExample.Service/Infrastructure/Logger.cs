using Common.Contracts.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerExample.Service.Infrastructure {
	public class Logger : ILogger {
		public void Error(MicroServiceException ex, string info) {
			Console.WriteLine($"[Error] {info} {ex.Message}");
		}

		public void Fatal(Exception e, string info) {
			Console.WriteLine($"[Fatal] {info} {e.Message}");
		}

		public void Warn(MicroServiceException ex, string info) {
			Console.WriteLine($"[Warn] {info} {ex.Message}");
		}

		public void Info(string info) {
			Console.WriteLine($"[Info] {info}");

		}

		public void Warn(string message) {
			Console.WriteLine($"[Warn] {message}");
		}
	}
}
