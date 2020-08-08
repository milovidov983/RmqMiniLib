using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
		Task SendRpc(string topic, byte[] payload, string correlationId);
		/// <summary>
		/// Send notify message
		/// </summary>
		/// <param name="topic">topic</param>
		/// <param name="payload">payload</param>
		Task SendNotify(string topic, byte[] payload);
	}
}
