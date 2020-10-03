using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class HubHandlersConfig : IHubHandlersConfig {
		private readonly IRabbitHub hub;
		private string queue;
		private IQueueHandlersConfig queueConfig;

		public HubHandlersConfig(IRabbitHub hub) {
			this.hub = hub;
		}

		public IHubHandlersConfig ForQueue(string queue, Func<IQueueHandlersConfig, IQueueHandlersConfig> builder) {
			this.queue = queue;
			queueConfig = builder.Invoke(new QueueHandlersConfig(hub));
			return this;
		}

		public async Task<ISubscription> Start() {
			// create in channel


			var subs = new Subscription(hub, queueConfig);
			await subs.Init();
			return subs;
		}
	}
}
