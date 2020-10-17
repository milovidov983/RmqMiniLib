using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib {
	public interface ISubscription : IDisposable {
		Task StopGracefully(CancellationToken gracefulToken);
	}
}
