using System;

namespace ServerExample.Contracts {
	public class ExampleCommand {
		public const string Topic = "exampleCommand.serverExample.rpc";

		public class Request {
			public string Message { get; set; }
		}

		public class Response {
			public string Message { get; set; }
		}
	}
}
