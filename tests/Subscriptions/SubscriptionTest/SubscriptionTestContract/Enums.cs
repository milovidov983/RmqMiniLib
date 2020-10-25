using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace SubscriptionTestContract {
	/// <summary>
	/// Статус обработки сообщения микросервисом
	/// </summary>
	[Flags]
	public enum StatusCodes {
		/// <summary>
		/// Обработано
		/// </summary>
		[Description("Обработано")]
		OK = 0x00,
		/// <summary>
		/// Некорректный запрос
		/// </summary>
		[Description("Некорректный запрос")]
		InvalidRequest = 0x01,
		/// <summary>
		/// Не найдено
		/// </summary>
		[Description("Не найдено")]
		NotFound = 0x02,
		/// <summary>
		/// Ошибка стороннего сервиса (smsc, sendgrid, etc.)
		/// </summary>
		[Description("Ошибка стороннего сервиса (smsc, sendgrid, etc.)")]
		ExternalError = 0x08,
		/// <summary>
		/// Ошибка стороннего сервиса (smsc, sendgrid, etc.)
		/// </summary>
		[Description("Ошибка предназначенная для демонстрации пользователю")]
		UserError = 0x10,
		/// <summary>
		/// Во время обработки запроса произошла внутрення ошибка микросервиса
		/// </summary>
		[Description("Во время обработки запроса произошла внутренняя ошибка микросервиса")]
		InternalError = 0x10000000
	}
}
