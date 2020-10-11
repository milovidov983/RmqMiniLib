using System;

namespace RmqLib2 {

	class PublishItem {
		public DeliveryInfo DeliveryInfo { get; }
		public PublishItem(DeliveryInfo deliveryInfo, Action<Exception> errorAction, Action successAction = null) {
			PublishErrorAction = errorAction;
			PublishSuccessAction = successAction;
			DeliveryInfo = deliveryInfo;
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

	
	}
}
