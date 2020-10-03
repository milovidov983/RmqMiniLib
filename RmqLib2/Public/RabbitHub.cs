using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RmqLib2 {


	public class RabbitHub : IRabbitHub {
		private IChanelFactory channelFactory;
		private IQueueHandlersConfig queueConfig;

		public RabbitHub(string connectionString) {
			// create connectionManager будет создавать соединение и следить
			// за его состоянием при необходимости переподключатся и создавать каналы
			var cm = new ConnectionManager(connectionString);
			channelFactory = cm.CreateChannelFactory();


		}


		internal void SetQueueHandlersConfig(IQueueHandlersConfig queueConfig) {
			this.queueConfig = queueConfig;
		}

		public Task<DeliveredMessage> ExecuteRpcAsync(DeliveryInfo deliveryInfo, Payload payload, CancellationToken token) {
			IRmqChanel outputChannel = channelFactory.CreateChanel();
			var task = new TaskCompletionSource<DeliveredMessage>(token);

			outputChannel.Send(deliveryInfo, payload, task);

			return task.Task;
		}

		public Task PublishAsync(DeliveryInfo deliveryInfo, Payload payload, CancellationToken token) {
			throw new NotImplementedException();
		}

		public void SubscribeAsync(string queueName, Func<DeliveredMessage, Task<MessageProcessResult>> onMessage, int prefetchCount = 32) {
			IRmqChanel inputChannel = channelFactory.GetInputChannel(queueName, prefetchCount);
			inputChannel.OnMessage = onMessage;

		}
	}
}
