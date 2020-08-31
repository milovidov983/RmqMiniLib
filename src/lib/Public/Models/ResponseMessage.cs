using System;
using System.Text;
using System.Text.Json;

namespace RmqLib {
	/// <summary>
	/// Для ответа в команде сервиса используем этот класс обёртку
	/// </summary>
	/// <typeparam name="TResult"></typeparam>
	public class ResponseMessage {
		public byte[] Result { get; private set; } //= new byte[] { 1, 0, 0, 0 };//Array.Empty<byte>();

		public static ResponseMessage Create<TResult>(TResult result) where TResult: class {
			if(result is null) {
				throw new ArgumentNullException($"Parameter {nameof(result)} is null!");
			}
			var res = JsonSerializer.Serialize(result);
			var resp = new ResponseMessage {
				Result = Encoding.UTF8.GetBytes(res.ToString())
			};
			return resp;
		}

		public static ResponseMessage Empty = new ResponseMessage();
	}
}
