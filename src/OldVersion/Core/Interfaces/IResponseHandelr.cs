using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	internal interface IResponseHandelr {
		void AddReplySubscription(string correlationId, TaskCompletionSource<string> completionSource);
		TaskCompletionSource<string> RemoveReplySubscription(string correlationId);
	}
}
