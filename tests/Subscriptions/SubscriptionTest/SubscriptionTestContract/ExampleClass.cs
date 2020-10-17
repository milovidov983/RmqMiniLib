using System;

namespace SubscriptionTest.Contract {
	public class ExampleClass {
		public const string Topic = "test.subscriptionTest.rpc";

		public class Request {
			public string Message { get; set; }
		}

		public class Response {
			public string Message { get; set; }
		}
	}
}
