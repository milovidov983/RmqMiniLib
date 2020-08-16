using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RmqLib {
	/// <summary>
	/// Ответственный за обработку исключений возникающих 
	/// в командах сервиса IRmqCommandHandler и IRmqNotificationHandler
	/// </summary>
	public interface IExceptionHandlerService {
		Task HandleException(Exception serviceException, DeliveredMessage dm);
	}
}
