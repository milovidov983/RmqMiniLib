using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	internal interface IRetryConnectionFactory {
		IRetryConnection Create(IConnection connection);
	}
}
