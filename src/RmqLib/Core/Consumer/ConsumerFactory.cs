using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RmqLib.Core;

namespace RmqLib {
	internal class ConsumerFactory : IConsumerFactory {
		private readonly IChannelWrapper channel;
		private readonly IConsumerBinder consumerBinder;

		public ConsumerFactory(IChannelWrapper channel, IConsumerBinder consumerBinder) {
			this.channel = channel;
			this.consumerBinder = consumerBinder;
		}

		public AsyncEventingBasicConsumer CreateBasicConsumer() {


			var consumerInstance = new AsyncEventingBasicConsumer(channel.GetTrueChannel());
			consumerBinder.Bind(consumerInstance, channel);
			return consumerInstance;
		}
	}
}
