using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RmqLib {
	public class JsonOptions {
		public static JsonSerializerOptions Default;

		static JsonOptions() {
			Default = new JsonSerializerOptions {
				PropertyNameCaseInsensitive = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				IgnoreNullValues = true,

			};

			Default.Converters.Add(new JsonStringEnumConverter());
		}
	}

    
}
