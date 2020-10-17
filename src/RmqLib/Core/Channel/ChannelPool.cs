using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ChannelPool: IChannelPool {
		private readonly IChannelWrapper channel;

		public ChannelPool(IModel channel) {
			this.channel = new ChannelWrapper(channel);
		}

		public IChannelWrapper GetChannel() {
			return channel;
		}
	}
}
