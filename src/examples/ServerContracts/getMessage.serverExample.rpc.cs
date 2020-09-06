using System;

namespace Server.Contracts {
	public class GetMessageExample {
		public const string Topic = "getMessage.serverExample.rpc";

		public class Request {
			public int Id { get; set; }
		}

		public class Response {
			public string Data { get; set; }
		}
	}
}