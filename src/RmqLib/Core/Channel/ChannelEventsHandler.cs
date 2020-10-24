using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace RmqLib.Core {
	class ChannelEventsHandler : IChannelEventsHandler {
		private IChannelPool channelPool; // без понятия зачем, 
		//наверное он будет посылать в него сигналы о том что все плохо и пора создавать другой канал

		public ChannelEventsHandler(IChannelPool channelPool) {
			this.channelPool = channelPool;
		}

		public void BasicAcks(object sender, BasicAckEventArgs e) {
			Console.WriteLine("ChannelEventsHandler BasicAcks ");
		}

		public void BasicNacks(object sender, BasicNackEventArgs e) {
			Console.WriteLine("ChannelEventsHandler BasicNacks ");
		}

		public void BasicRecoverOk(object sender, EventArgs e) {
			Console.WriteLine("ChannelEventsHandler BasicRecoverOk");
		}

		public void BasicReturn(object sender, BasicReturnEventArgs e) {
			Console.WriteLine("ChannelEventsHandler BasicReturn " + e.ReplyText);
		}

		public void CallbackException(object sender, CallbackExceptionEventArgs e) {
			Console.WriteLine("ChannelEventsHandler CallbackException " + e.Exception.Message);
		}

		public void FlowControl(object sender, FlowControlEventArgs e) {
			Console.WriteLine("ChannelEventsHandler FlowControl");
		}

		public void ModelShutdown(object sender, ShutdownEventArgs e) {
			Console.WriteLine("ChannelEventsHandler ModelShutdown " + e.ReplyText);
		}
	}
}
