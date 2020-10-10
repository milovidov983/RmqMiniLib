using System;

namespace RmqLib {
	/// <summary>
	/// Exception использующийся в библиотеке
	/// </summary>
	public class RmqException : Exception {
		/// <summary>
		/// Код ошибки
		/// </summary>
		public int StatusCode { get; protected set; }

		/// <summary>
		/// Имя хоста где случилась ошибка
		/// </summary>
		internal string HostName { get; private set; }
		/// <summary>
		/// Имя сервиса в котором случилась ошибка
		/// </summary>
		internal string ServiceName { get; private set; }

		public RmqException(string message, int statusCode) : base(message) {
			StatusCode = statusCode;
		}

		public RmqException(string message, Exception innerException, int statusCode) : base(message, innerException) {
			StatusCode = statusCode;
		}


		/// <summary>
		/// Добавить пользовательскую информацию в key/value коллекцию Exception.Data.
		/// Одинаковые keys игнорируются
		/// </summary>
		public RmqException AddAdditionalInformation(string key, string value = "") {
			if (string.IsNullOrEmpty(key)) {
				throw new ArgumentException($"Parameter \"{nameof(key)}\" is null or empty", nameof(key));
			}

			if (!Data.Contains(key)) {
				Data.Add(key, value);
			} 

			return this;
		}
	}
}
