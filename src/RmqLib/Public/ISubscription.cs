using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib {
	public interface ISubscription : IDisposable {
		Task StopGracefully(CancellationToken gracefulToken);
	}


	public class Subscription : ISubscription {
		public void Dispose() {
			//TODO
		}

		public Task StopGracefully(CancellationToken gracefulToken) {
			//TODO
			return Task.CompletedTask;
		}
	}
}
