using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal class ChannelPool: IChannelPool , IDisposable {
		private readonly IModel trueChannel;
		private readonly IChannelWrapper wrappedChannel;
		private Action<IModel> unbindEventHandlers;

		public ChannelPool(IModel channel) {
			this.trueChannel = channel;
			this.wrappedChannel = new ChannelWrapper(channel);
		}


		public void BindEventHandlers(Action<IModel> action) {
			action.Invoke(trueChannel);
		}

		public void RegisterUnsubscribeAction(Action<IModel> action) {
			this.unbindEventHandlers = action;
		}

		public IChannelWrapper GetChannel() {
			return wrappedChannel;
		}

		public void Dispose() {
			if(trueChannel != null) {
				unbindEventHandlers.Invoke(trueChannel);
				trueChannel.Close();
			}
		}
	}
}
