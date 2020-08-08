using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	/// <summary>
	/// TODO comment
	/// </summary>
	internal interface IChannelFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		IChannel Create(IReplyHandler handler);
	}
}
