﻿using Microsoft.Extensions.Logging;

namespace RmqLib.Factories {
	/// <summary>
	/// TODO comment
	/// </summary>
	public class ConnectionFactory: IConnectionFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly RmqConfig rmqConfig;
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly EventHandlers connectionEventHandlers;
		/// <summary>
		/// TODO comment
		/// </summary>
		private readonly ILogger logger;

		/// <summary>
		/// TODO comment
		/// </summary>
		public ConnectionFactory(RmqConfig rmqConfig, EventHandlers connectionEventHandlers = null, ILogger logger = null) {
			this.rmqConfig = rmqConfig;
			this.connectionEventHandlers = connectionEventHandlers;
			this.logger = logger;
		}
		/// <summary>
		/// TODO comment
		/// </summary>
		public IConnection Create() {
			return new Connection(rmqConfig, connectionEventHandlers, logger);
		}
	}
}