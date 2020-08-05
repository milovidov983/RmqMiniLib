using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {
	/// <summary>
	/// TODO comment
	/// </summary>
	public interface IConnection {
		/// <summary>
		/// TODO comment
		/// </summary>
		RabbitMQ.Client.IConnection RmqConnection { get; }
		/// <summary>
		/// TODO comment
		/// </summary>
		void ConnectToRmq(bool reconnectIfFailed = true);
		/// <summary>
		/// TODO comment
		/// </summary>
		void RetryConnection();
	}
}
