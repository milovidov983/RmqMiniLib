using System;

namespace ServerExample.Contracts {
	public class BroadcastCommand {
		public const string Topic = "broadcastCommand.serverExample.none.cs";

		public class Message {
			public string Body { get; set; }
		}
	}
}
