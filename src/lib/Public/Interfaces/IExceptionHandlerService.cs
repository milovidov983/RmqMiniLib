using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	public interface IExceptionHandlerService {
		Task HandleException(Exception serviceException, DeliveredMessage dm);
	}
}
