using RabbitMQ.Client;
using System;

namespace RmqLib.Core {

	class PublishItem {
		public string Exchange { get; }
		public string RoutingKey { get; }
		public ReadOnlyMemory<byte> Body { get; }
		public string CorrelationId { get; }
		public string ReplyTo { get; }
		public string AppId { get; }



		public PublishItem(DeliveryInfo deliveryInfo, Action<Exception> errorAction, Action successAction = null) {
			PublishErrorAction = errorAction;
			PublishSuccessAction = successAction;

			CorrelationId = deliveryInfo.CorrelationId;
			ReplyTo = deliveryInfo.ReplyTo;
			AppId = DeliveryInfo.AppId;
			Exchange = DeliveryInfo.ExhangeName;
			RoutingKey = deliveryInfo.Topic;
			Body = deliveryInfo.Body;
		}

		/// <summary>
		/// Функция вызывается при ошибке публикации сообщения
		/// </summary>
		public Action<Exception> PublishErrorAction { get; }
		/// <summary>
		/// Функция вызывается успешной публикации
		/// </summary>
		public Action PublishSuccessAction { get; }


		/// <summary>
		/// Флаг определяет стоит ли пытаться опубликовать это сообщение если это не было сделано
		/// </summary>
		public bool IsCanceled { get; set; }

		public void Abort() {
			IsCanceled = true;
		}
	}
}
