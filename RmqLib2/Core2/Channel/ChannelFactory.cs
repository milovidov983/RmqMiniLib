using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib2 {
	internal class ChannelFactory: IChannelFactory {
		private readonly IChannelWrapper channel;

		public ChannelFactory(IModel channel, IReplyHandler replyHandler) {
			this.channel = new ChannelWrapper(channel, replyHandler);
		}

		public IChannelWrapper GetChannel() {
			return channel;
		}

		public Task InitChannel(IModel channel) {
			return this.channel.SetChannel(channel);
		}
	}
}
