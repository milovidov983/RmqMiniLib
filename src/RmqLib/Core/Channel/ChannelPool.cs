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


		public void BindEventHandlers(Action<IModel> bindEvent) {
			bindEvent.Invoke(trueChannel);
		}

		public void UnBindEventHandlers(Action<IModel> unBindEvent) {
			this.unbindEventHandlers = unBindEvent;
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
