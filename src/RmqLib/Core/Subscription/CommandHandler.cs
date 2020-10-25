using RabbitMQ.Client;

namespace RmqLib {

	public class CommandHandler {
		public IRabbitCommand Handler { get; set; }
		public string Topic { get; set; }
	}
	
}
