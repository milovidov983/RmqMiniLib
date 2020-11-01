namespace RmqLib.Core {
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
