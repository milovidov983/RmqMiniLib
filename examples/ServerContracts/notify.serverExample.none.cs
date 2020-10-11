using System;

namespace Server.Contracts {
	public class NotifyExample {
		public const string Topic = "notify.serverExample.none";

		public class Message {
			public string Data { get; set; }
		}

	}
}