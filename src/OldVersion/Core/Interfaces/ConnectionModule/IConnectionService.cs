using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	/// <summary>
	/// TODO comment
	/// </summary>
	public interface IConnectionService {
		/// <summary>
		/// TODO comment
		/// </summary>
		RabbitMQ.Client.IConnection RmqConnection { get; }
		/// <summary>
		/// TODO comment
		/// </summary>
		void StartConnection(bool reconnectIfFailed = true);
		/// <summary>
		/// 
		/// </summary>
		bool IsConnected { get; }
		/// <summary>
		/// 
		/// </summary>
		void CreateRmqConnection();
	}
}
