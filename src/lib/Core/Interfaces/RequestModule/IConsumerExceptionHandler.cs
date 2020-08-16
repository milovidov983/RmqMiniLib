using System;
using System.Threading.Tasks;

namespace RmqLib.Core {
	internal interface IConsumerExceptionHandler {
		Task HandleException(DeliveredMessage dm, Exception serviceException);
	}
}