using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	public enum MessageProcessResult {
        Ack = 1,
        Requeue,
        Reject
    }
}
