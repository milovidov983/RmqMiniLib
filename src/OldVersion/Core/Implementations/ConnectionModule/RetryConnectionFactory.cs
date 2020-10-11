using Microsoft.Extensions.Logging;
using RmqLib.Core;

namespace RmqLib.Factories {
	/// <summary>
	/// TODO comment
	/// </summary>
	internal class RetryConnectionFactory : IRetryConnectionFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly ILogger logger;

		/// <summary>
		/// TODO comment
		/// </summary>
		public RetryConnectionFactory(ILogger logger) {
			this.logger = logger;
		}
		/// <summary>
		/// TODO comment
		/// </summary>
		public IRetryConnection Create(IConnectionService connection) {
			return new RetryConnection(connection, logger);
		}
	}
}
