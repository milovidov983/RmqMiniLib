using RabbitMQ.Client;

namespace RmqLib {

	public class CommandHandler {
		public IRabbitCommand Command { get; set; }
		public string Topic { get; set; }
	}
	
}
