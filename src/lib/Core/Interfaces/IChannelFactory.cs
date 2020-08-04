using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {
	/// <summary>
	/// TODO comment
	/// </summary>
	public interface IChannelFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		IChannel Create();
	}
}
