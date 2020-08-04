using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {
	/// <summary>
	/// Channel
	/// </summary>
	public interface IChannel {
		/// <summary>
		/// Send RPC request
		/// </summary>
		/// <param name="topic">topic</param>
		/// <param name="payload">payload</param>
		/// <returns>correlationId</returns>
		string SendRpc(string topic, byte[] payload);
	}
}
