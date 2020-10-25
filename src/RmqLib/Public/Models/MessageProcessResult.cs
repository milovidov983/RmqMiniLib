using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
    public sealed class MessageProcessResult {
        public static readonly MessageProcessResult Ack = new MessageProcessResult();
        public static readonly MessageProcessResult Requeue = new MessageProcessResult();
        public static readonly MessageProcessResult Reject = new MessageProcessResult();
        public static readonly Task<MessageProcessResult> AckTask = Task.FromResult<MessageProcessResult>(MessageProcessResult.Ack);
        public static readonly Task<MessageProcessResult> RequeueTask = Task.FromResult<MessageProcessResult>(MessageProcessResult.Requeue);
        public static readonly Task<MessageProcessResult> RejectTask = Task.FromResult<MessageProcessResult>(MessageProcessResult.Reject);

        public Task<MessageProcessResult> AsTask {
            get {
                if (this == MessageProcessResult.Ack)
                    return MessageProcessResult.AckTask;
                if (this != MessageProcessResult.Requeue)
                    return MessageProcessResult.RejectTask;
                return MessageProcessResult.RequeueTask;
            }
        }

        private MessageProcessResult() {
        }
    }
}
