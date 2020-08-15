using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	public delegate Task ExceptionHandler(Exception ex, DeliveredMessage deliveredMessage);
}
