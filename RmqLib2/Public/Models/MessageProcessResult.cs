using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib2 {
	public sealed class MessageProcessResult {
		public static readonly MessageProcessResult Ack = new MessageProcessResult();
		public static readonly MessageProcessResult Requeue = new MessageProcessResult();
		public static readonly MessageProcessResult Reject = new MessageProcessResult();
		public static readonly Task<MessageProcessResult> AckTask = Task.FromResult(new MessageProcessResult());
		public static readonly Task<MessageProcessResult> RequeueTask = Task.FromResult(new MessageProcessResult());
		public static readonly Task<MessageProcessResult> RejectTask = Task.FromResult(new MessageProcessResult());
	}
}
