using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace RmqLib.Core {
	class ChannelEventsHandler : IChannelEventsHandler {
		private readonly IRmqLogger logger;

		public ChannelEventsHandler(IRmqLogger logger) {
			this.logger = logger;
		}

		public void BasicAcks(object sender, BasicAckEventArgs e) {
			logger.Debug($"{nameof(ChannelEventsHandler)} {nameof(BasicAcks)} ");
		}

		public void BasicNacks(object sender, BasicNackEventArgs e) {
			logger.Debug($"{nameof(ChannelEventsHandler)} {nameof(BasicNacks)} ");
		}

		public void BasicRecoverOk(object sender, EventArgs e) {
			logger.Debug($"{nameof(ChannelEventsHandler)} {nameof(BasicRecoverOk)}");
		}

		public void BasicReturn(object sender, BasicReturnEventArgs e) {
			logger.Debug($"{nameof(ChannelEventsHandler)} {nameof(BasicReturn)} {nameof(e.ReplyText)}: {e.ReplyText}");
		}

		public void CallbackException(object sender, CallbackExceptionEventArgs e) {
			logger.Debug($"{nameof(ChannelEventsHandler)} {nameof(CallbackException)} Exception message: {.Exception.Message}");
		}

		public void FlowControl(object sender, FlowControlEventArgs e) {
			logger.Debug($"{nameof(ChannelEventsHandler)} {nameof(FlowControl)}");
		}

		public void ModelShutdown(object sender, ShutdownEventArgs e) {
			logger.Debug($"{nameof(ChannelEventsHandler)} {nameof(ModelShutdown)} {nameof(e.ReplyText)}: {e.ReplyText});
		}
	}
}
