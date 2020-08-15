using Microsoft.Extensions.Logging;
using RmqLib.Core;

namespace RmqLib.Factories {
	/// <summary>
	/// TODO comment
	/// </summary>
	internal class ConnectionFactory: IConnectionFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly RmqConfig rmqConfig;
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
		public ConnectionFactory(RmqConfig rmqConfig, ILogger logger = null) {
			this.rmqConfig = rmqConfig;
			this.logger = logger;
			this.retryConnectionFactory = new RetryConnectionFactory(logger);
		}
		/// <summary>
		/// TODO comment
		/// </summary>
		public IConnectionService Create() {
			return new ConnectionService(rmqConfig, retryConnectionFactory, logger);
		}
	}
}
