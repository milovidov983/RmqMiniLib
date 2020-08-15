using System;

namespace ExampleContracts1 {
	public class SumExampleCommand {
		public const string Topic = "sum.exampleService1.rpc";

		public class Request {
			public int A { get; set; }
			public int B { get; set; }
		}

		public class Response {
			public int Sum { get; set; }
		}
	}
}