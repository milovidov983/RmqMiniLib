using RmqLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	public static class FluentSubscription {
		public static IHubHandlersConfig DefineHandlers(this IRabbitHub hub, int prefetchCount = 32) {
			return new HubHandlersConfig((RabbitHub)hub);
		}


	}

	public interface IHubHandlersConfig {
		IHubHandlersConfig ForQueue(string queue, Func<IQueueHandlersConfig, IQueueHandlersConfig> builder);
		Task<ISubscription> Start();
	}


	public class HubHandlersConfig : IHubHandlersConfig {
		private readonly RabbitHub hub;
		private QueueHandlersConfig queueHandlersConfig;

		public HubHandlersConfig(RabbitHub hub) {
			this.hub = hub ?? throw new ArgumentNullException(nameof(hub));
		}

		public IHubHandlersConfig ForQueue(string queue, Func<IQueueHandlersConfig, IQueueHandlersConfig> builder) {
			Validate(queue, builder);
			queueHandlersConfig = new QueueHandlersConfig(hub);
			builder.Invoke(queueHandlersConfig);

			return this;

		}

		private static void Validate(string queue, Func<IQueueHandlersConfig, IQueueHandlersConfig> builder) {
			if (queue is null) {
				throw new ArgumentNullException(nameof(queue));
			}
			if (builder is null) {
				throw new ArgumentNullException(nameof(builder));
			}
		}

		public Task<ISubscription> Start() {
			var subscriptionManager = hub.CreateSubscriptions(queueHandlersConfig.commandHandlers);
			subscriptionManager.AddHandler(queueHandlersConfig);

			// TODO потом что то тут сделать типо корректного закрытия соединения
			return Task.FromResult<ISubscription>(new Subscription());
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
