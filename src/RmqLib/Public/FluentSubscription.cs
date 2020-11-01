using RmqLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	public static class FluentSubscription {
		public static IHubHandlersConfig DefineHandlers(this IRabbitHub hub) {
			return new HubHandlersConfig((RabbitHub)hub);
		}
	}

	public interface IHubHandlersConfig {
		IHubHandlersConfig AddHandlers(Func<IQueueHandlersConfig, IQueueHandlersConfig> builder);
		Task<ISubscription> Start();
	}


	public class HubHandlersConfig : IHubHandlersConfig {
		private readonly RabbitHub hub;
		private QueueHandlersConfig queueHandlersConfig;

		public HubHandlersConfig(RabbitHub hub) {
			this.hub = hub ?? throw new ArgumentNullException(nameof(hub));
		}

		public IHubHandlersConfig AddHandlers(Func<IQueueHandlersConfig, IQueueHandlersConfig> builder) {
			Validate(builder);
			queueHandlersConfig = new QueueHandlersConfig(hub);
			builder.Invoke(queueHandlersConfig);

			return this;

		}

		private static void Validate(Func<IQueueHandlersConfig, IQueueHandlersConfig> builder) {
			if (builder is null) {
				throw new ArgumentNullException(nameof(builder));
			}
		}

		public Task<ISubscription> Start() {
			var subscriptionManager = hub.CreateSubscriptions(queueHandlersConfig.commandHandlers);
			subscriptionManager.AddHandler(queueHandlersConfig);
			return Task.FromResult<ISubscription>(new Subscription(hub));
		}
	}


	public interface IQueueHandlersConfig {
		IQueueHandlersConfig AfterExecute(Func<DeliveredMessage, MessageProcessResult, Task<MessageProcessResult>> handler);
		IQueueHandlersConfig BeforeExecute(Func<DeliveredMessage, Task<bool>> handler);
		IQueueHandlersConfig OnException(Func<Exception, DeliveredMessage, Task<bool>> handler);
		IQueueHandlersConfig OnUnexpectedTopic(Func<DeliveredMessage, Task<MessageProcessResult>> handler);
		IQueueHandlersConfig OnTopic(string topic, IRabbitCommand command);
	}
}
