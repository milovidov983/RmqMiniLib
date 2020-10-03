using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class Subscription : ISubscription {

		private IRabbitHub hub;
		private IQueueHandlersConfig queueConfig;

		public Subscription(IRabbitHub hub, IQueueHandlersConfig queueConfig) {
			this.hub = hub;
			this.queueConfig = queueConfig;
		}

		public void Dispose() {
			//throw new NotImplementedException();
		}

		public Task StopGracefully(CancellationToken gracefulToken) {
			//throw new NotImplementedException();
			return Task.CompletedTask;
		}

		internal Task Init() {
			hub.SubscribeAsync()

		}
	}
}
