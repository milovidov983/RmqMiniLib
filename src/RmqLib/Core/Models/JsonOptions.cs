using System.Text.Json;

namespace RmqLib.Core {
	public class JsonOptions {
		public static JsonSerializerOptions Default = new JsonSerializerOptions {
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			IgnoreNullValues = true
		};
	}
}
