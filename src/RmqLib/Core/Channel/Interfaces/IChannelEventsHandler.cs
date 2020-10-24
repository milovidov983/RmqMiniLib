using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;

namespace RmqLib.Core {
	interface IChannelEventsHandler {
		void FlowControl(object sender, FlowControlEventArgs e);

		void BasicAcks(object sender, BasicAckEventArgs e);

		void BasicNacks(object sender, BasicNackEventArgs e);

		void BasicRecoverOk(object sender, EventArgs e);

		void BasicReturn(object sender, BasicReturnEventArgs e);

		void CallbackException(object sender, CallbackExceptionEventArgs e);

		void ModelShutdown(object sender, ShutdownEventArgs e);
	}
}
