using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace RmqLib.Core {
	/// <summary>
	/// Внутренний класс для более удобной работы  с ошибками
	/// </summary>
	internal class Error {
		/// <summary>
		/// По умолчанию если статус пуст то возвращаем статус который обозначен у нас таким описанием:
		/// "Во время обработки запроса произошла внутренняя ошибка микросервиса"
		/// </summary>
		public const int INTERNAL_ERROR = 0x10000000;
		/// <summary>
		/// Некорректный запрос
		/// </summary>
		public const int INVALID_REQUEST_CODE = 0x01;
		
	}
}
