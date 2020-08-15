using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {
	/// <summary>
	/// Создает клиентские обработчики для событий
	/// </summary>
	public interface IEventHandlersFactoryService {
		/// <summary>
		/// Обработчики событий соединения
		/// </summary>
		ConnectionEventHandlers CreateConnectionEventHandlers();
	}
}
