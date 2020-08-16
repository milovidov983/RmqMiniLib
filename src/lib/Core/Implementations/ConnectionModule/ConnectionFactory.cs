using Microsoft.Extensions.Logging;
using RmqLib.Core;
using RmqLib.Factories;

namespace RmqLib.Core {
	/// <summary>
	/// TODO comment
	/// </summary>
	internal class ConnectionFactory : IConnectionFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly RmqConfig rmqConfig;
		private readonly IConnectionEvents connectionEvents;

		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly ILogger logger;
		/// <summary>
		/// 
		/// </summary>
		private readonly IRetryConnectionFactory retryConnectionFactory;

		/// <summary>
		/// TODO comment
		/// </summary>
		public ConnectionFactory(RmqConfig rmqConfig, IConnectionEvents connectionEvents, ILogger logger = null) {
			this.rmqConfig = rmqConfig;
			this.connectionEvents = connectionEvents;
			this.logger = logger;
			this.retryConnectionFactory = new RetryConnectionFactory(logger);
		}
		/// <summary>
		/// TODO comment
		/// </summary>
		public IConnectionService Create() {
			return new ConnectionService(rmqConfig, retryConnectionFactory, connectionEvents, logger);
		}
	}
}
