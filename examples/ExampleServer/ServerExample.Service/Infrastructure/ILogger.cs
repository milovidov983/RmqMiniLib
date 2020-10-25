using Common.Contracts.Infrastructure;
using System;

namespace ServerExample.Service.Infrastructure {
	public interface ILogger {
		void Error(MicroServiceException ex, string info);
		void Warn(MicroServiceException ex, string info);
		void Fatal(Exception e, string info);
		void Info(string info);
		void Warn(string message);
	}
}