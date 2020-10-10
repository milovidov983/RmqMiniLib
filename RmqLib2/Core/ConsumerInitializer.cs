using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib2 {

	internal class ConsumerInitializer {
		private IModel channel;
		private IReplyHandler replyHandler;
		private IConnectionManager channelRecovery;

		public ConsumerInitializer(IModel channel, IReplyHandler replyHandler, IConnectionManager channelRecovery) {
			this.channel = channel;
			this.replyHandler = replyHandler;
			this.channelRecovery = channelRecovery;
		}

		public void InitConsumer() {
			var consumer = new AsyncEventingBasicConsumer(channel);
			channel.BasicConsume(
				consumer: consumer,
				queue: ServiceConstants.REPLY_QUEUE_NAME,
				autoAck: true);
			consumer.Received += replyHandler.ReceiveReply;
			consumer.Registered += channelRecovery.ConsumerRegistred;

			// TODO пересоздавать коньсюмера при неожиданном Shutdown
		}
	}
}
