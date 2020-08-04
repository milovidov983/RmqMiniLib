using System;
using System.Collections.Generic;
using System.Text;

namespace RmqLib {

	/// <summary>
	/// TODO comment
	/// </summary>
	internal static class ServiceConstants {
		// TODO translate
		/// <summary>
		/// Топики с признаком "rpc" библиотека обрабатывает как
		/// команды которые должны вернуть ответ, в отличие от Топиков оповещения
		/// заканчивающихся на "none" где ответ не отсылается.
		/// </summary>
		public const string PPC_TOKEN_TOPIC = "rpc";

		/// <summary>
		/// Топиков оповещения, ответ не отсылается.
		/// </summary>
		public const string NOTIFICATION_TOKEN_TOPIC = "none";

		/// <summary>
		/// Брудкаст обмен rmq
		/// </summary>
		public const string FANOUT_EXCHANGE = "amq.fanout";

		/// <summary>
		/// REPLY_QUEUE_NAME
		/// </summary>
		public const string REPLY_QUEUE_NAME = "amq.rabbitmq.reply-to";
	}
}
