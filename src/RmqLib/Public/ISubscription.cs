using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib {
	public interface ISubscription : IDisposable {
		void StopGracefully();
	}


	public class Subscription : ISubscription {
		private readonly RabbitHub hub;

		public Subscription(RabbitHub hub) {
			this.hub = hub;
		}

		public void Dispose() {
			hub.Close();
		}

		public void StopGracefully() {
			hub.Close();
		}
	}
}
