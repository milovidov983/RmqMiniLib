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
		/// <summary>
		/// Сообщение с описанием ошибки
		/// </summary>
		public string ErrorMessage { get; }
		/// <summary>
		/// Код статуса
		/// </summary>
		public int StatusCode { get; }
		/// <summary>
		/// Дополнительные данные от сервиса в котором случился exception
		/// </summary>
		public IDictionary<string, object> Data { get; }
		/// <summary>
		/// Имя хоста на котором случился exception
		/// </summary>
		public string Host { get; }
		/// <summary>
		/// Имя сервиса на в котором случился exception
		/// </summary>
		public string ServiceName { get; }

		/// <summary>
		/// Флаг true если есть ошибка
		/// </summary>
		public bool HasError { get => ErrorMessage != null; }

		public Error(IDictionary<string, object> headers) {
			ErrorMessage = GetErrorMessage(headers);
			if (ErrorMessage != null) {
				StatusCode = GetStatusCode(headers);
				Data = GetUserData(headers);
				Host = GetHost(headers);
				ServiceName = GetServiceName(headers);
			}
		}

		private string GetErrorMessage(IDictionary<string, object> headers) {
			if (headers != null && headers.TryGetValue(Headers.Error, out var error)) {
				return Encoding.UTF8.GetString(InternalHelpers.ObjectToByteArray(error));
			}
			return null;
		}

		private static readonly Dictionary<string, object> empty = new Dictionary<string, object>();
		private IDictionary<string,object> GetUserData(IDictionary<string, object> headers) {
			if (headers != null && headers.TryGetValue(Headers.CustomData, out var data)) {
				string raw = string.Empty;
				try {
					raw = Encoding.UTF8.GetString(InternalHelpers.ObjectToByteArray(data));
					return JsonSerializer.Deserialize<IDictionary<string, object>>(raw);
				} catch(Exception e)  {
					var errorMessage = "There is still data from the service attached " +
						"to the error(-x-data header), but we could not get it";

					return new Dictionary<string, object>() {
						["UserDataError"] = $"{errorMessage}. Failed to deserialize user exception data: {e.Message}",
						["RawUserErrorData"] = raw
					};
				}
			}
			return empty;
		}		
		
		private string GetHost(IDictionary<string, object> headers) {
			if (headers != null && headers.TryGetValue("-x-host", out var hostName)) {
				return Encoding.UTF8.GetString(InternalHelpers.ObjectToByteArray(hostName));
			}
			return "unknown host";
		}

		private string GetServiceName(IDictionary<string, object> headers) {
			if (headers != null && headers.TryGetValue("-x-service", out var serviceName)) {
				return Encoding.UTF8.GetString(InternalHelpers.ObjectToByteArray(serviceName));
			}
			return "unknown service name";
		}

		private int GetStatusCode(IDictionary<string, object> headers) {
			var hasStatusCode = headers.TryGetValue("-x-status-code", out var statusCodeObj);
			if (hasStatusCode) {
				var encodedStatusCode = Encoding.UTF8.GetString(InternalHelpers.ObjectToByteArray(statusCodeObj));
				var isParsed = int.TryParse(encodedStatusCode, out var statusCode);
				if (isParsed) {
					return statusCode;
				}
			}
			return INTERNAL_ERROR;
		}
	}
}
