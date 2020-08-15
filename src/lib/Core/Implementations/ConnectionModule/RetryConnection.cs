using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RmqLib.Core {


	internal class RetryConnection : IRetryConnection {
		private const int START_TIMEOUT_MS = 1000;


		private IConnection connection;
		private ILogger logger;

		public RetryConnection(IConnection connection, ILogger logger) {
			this.connection = connection;
			this.logger = logger;
		}

		/// <summary>
		/// TODO comment
		/// </summary>
		public void Retry() {
			int timeoutMs = START_TIMEOUT_MS;
			var d = 1;
			while (!connection.IsConnected) {
				Thread.Sleep(timeoutMs);
				try {
					connection.CreateConnection();
				} catch (Exception ex) {
					timeoutMs += 1000 / d++;
					logger?.LogWarning(
						$"Reconnection failed: {ex.Message}, " +
						$"the next attempt will reconnect in {timeoutMs / 1000} seconds");
				}
			}
		}
	}
}
