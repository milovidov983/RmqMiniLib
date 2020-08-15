using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib.Core {
	/// <summary>
	/// TODO comment
	/// </summary>
	internal interface IChannelFactory {
		/// <summary>
		/// TODO comment
		/// </summary>
		IChannel Create(IReplyHandler handler, string[] topics = null);

		/// <summary>
		/// Привязать обработчик входящих запросов
		/// </summary>
		/// <param name="topics">список топиков</param>
		/// <param name="requestHandler">Обработчик входящих команд из шины</param>
		void BindRequestHandler(List<string> topics, IRequestHandler requestHandler);
	}
}
