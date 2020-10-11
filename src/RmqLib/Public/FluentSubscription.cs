using RmqLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib2 {
	public static class FluentSubscription {
		public static IHubHandlersConfig DefineHandlers(this IRabbitHub hub, int prefetchCount = 32) {
			//var config = new HubHandlersConfig(hub);
			//return config;

			throw new NotImplementedException();
		}


	}

	public interface IHubHandlersConfig {
		IHubHandlersConfig ForQueue(string queue, Func<IQueueHandlersConfig, IQueueHandlersConfig> builder);
		Task<ISubscription> Start();
	}


	public interface IQueueHandlersConfig {
		IQueueHandlersConfig AfterExecute(Func<ResponseMessage, MessageProcessResult, MessageProcessResult> handler);
		IQueueHandlersConfig BeforeExecute(Func<ResponseMessage, bool> handler);
		IQueueHandlersConfig OnException(Func<Exception, ResponseMessage, bool> handler);
		IQueueHandlersConfig OnTopic(string topic, IRabbitCommand command);
	}
}
