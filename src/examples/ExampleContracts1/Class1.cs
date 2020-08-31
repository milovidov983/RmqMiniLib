using System;

namespace ExampleContracts1 {
	public class SumExampleCommand {
		public const string Topic = "getMessage.aerverExample.rpc";

		public class Request {
			public int Id { get; set; }
		}

		public class Response {
			public string Data { get; set; }
		}
	}
}